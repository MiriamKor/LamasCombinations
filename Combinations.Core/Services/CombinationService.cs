using System.Numerics;
using Combinations.Models.Internal;
using Combinations.Models.Responses;
using Combinations.Models.Settings;
using Microsoft.Extensions.Options;

namespace Combinations.Services;

public interface ICombinationService
{
    BigInteger Start(CombinationState state, int n);
    CombinationResponse GetNext(CombinationState state);
    CombinationsPageResponse GetAll(CombinationState state, int pageSize, long page = 1);
    CombinationResponse Back(CombinationState state);
    void Reset(CombinationState state);
}

    public class CombinationService(IOptions<CombinationSettings> options, IPermutationAlgorithms _algorithms) : ICombinationService
{
    private const string NoMoreMessage = "No more combinations.";

    private readonly CombinationSettings _settings = options.Value;

    public BigInteger Start(CombinationState state, int n)
    {
        int min = Math.Max(_settings.MinN, 1);
        int max = Math.Min(_settings.MaxN, _algorithms.MaxN);

        if (n < min || n > max)
            throw new ArgumentOutOfRangeException(nameof(n), $"n must be between {min} and {max}.");

        state.Clear();
        state.IsStarted = true;
        state.N = n;
        state.Total = _algorithms.Factorial(n);
        return state.Total;
    }

    public CombinationResponse GetNext(CombinationState state)
    {
        EnsureStarted(state);

        if (state.NextIndex >= state.Total)
            return new CombinationResponse { Message = NoMoreMessage };

        int[] permutation;
        if (state.NextIndex.IsZero || state.LastPermutation is null)
        {
            permutation = state.NextIndex.IsZero
                ? _algorithms.First(state.N)
                : _algorithms.GetByIndex(state.NextIndex, state.N);
        }
        else
        {
            permutation = (int[])state.LastPermutation.Clone();
            _algorithms.NextPermutation(permutation);
        }

        var index = state.NextIndex;
        state.LastPermutation = permutation;
        state.LastDisplayedIndex = index;
        state.NextIndex = index + 1;
        state.AllModeAnchor = null; 

        return ToResponse(index, permutation);
    }

    public CombinationsPageResponse GetAll(CombinationState state, int pageSize, long page = 1)
    {
        EnsureStarted(state);

        if (pageSize < 1 || pageSize > _settings.MaxPageSize)
            throw new ArgumentOutOfRangeException(nameof(pageSize), $"pageSize must be between 1 and {_settings.MaxPageSize}.");
        if (page < 1)
            throw new ArgumentOutOfRangeException(nameof(page), "page must be at least 1.");

        var anchor = BigInteger.Zero;

        var response = new CombinationsPageResponse
        {
            TotalCombinations = state.Total.ToString(),
            PageSize = pageSize,
            CurrentPage = page.ToString()
        };

        var remaining = state.Total - anchor;
        if (remaining <= 0)
        {
            response.Message = NoMoreMessage;
            return response;
        }

        var totalPages = (remaining + pageSize - 1) / pageSize;
        response.TotalPages = totalPages.ToString();

        if (page > totalPages)
            throw new ArgumentOutOfRangeException(nameof(page), $"page must be between 1 and {totalPages}.");

        var start = anchor + (page - 1) * pageSize;
        var end = BigInteger.Min(start + pageSize, state.Total);

        var permutation = _algorithms.GetByIndex(start, state.N);
        for (var i = start; i < end; i++)
        {
            response.Combinations.Add(ToResponse(i, permutation));
            if (i + 1 < end)
            {
                permutation = (int[])permutation.Clone();
                _algorithms.NextPermutation(permutation);
            }
        }

        state.LastDisplayedIndex = end - 1;
        return response;
    }

    public CombinationResponse Back(CombinationState state)
    {
        EnsureStarted(state);

        if (state.LastDisplayedIndex is not { } last)
            return new CombinationResponse { Message = "Nothing has been displayed yet." };

        if (last <= 0)
            return new CombinationResponse { Message = "Already at the first combination." };

        var target = last - 1;
        var permutation = _algorithms.GetByIndex(target, state.N);

        state.LastDisplayedIndex = target;
        state.NextIndex = target + 1;
        state.LastPermutation = permutation;
        state.AllModeAnchor = null;

        return ToResponse(target, permutation);
    }

    public void Reset(CombinationState state) => state.Clear();

    private static void EnsureStarted(CombinationState state)
    {
        if (!state.IsStarted)
            throw new NotStartedException();
    }

    private static CombinationResponse ToResponse(BigInteger index, int[] permutation) => new()
    {
        SerialNumber = (index + 1).ToString(),
        Combination = permutation.ToList()
    };
}
