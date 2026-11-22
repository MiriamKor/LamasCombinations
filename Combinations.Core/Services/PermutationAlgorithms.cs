using System.Numerics;

namespace Combinations.Services;

public interface IPermutationAlgorithms
{
    int MaxN { get; }
    BigInteger Factorial(int n);
    int[] First(int n);
    int[] GetByIndex(BigInteger index, int n);
    bool NextPermutation(int[] a);
}

public class PermutationAlgorithms : IPermutationAlgorithms
{
    private const int MaxFactorialN = 20;
    private static readonly BigInteger[] Factorials = BuildFactorials();
    public int MaxN => MaxFactorialN;

    public BigInteger Factorial(int n)
    {
        if (n < 0 || n > MaxFactorialN)
            throw new ArgumentOutOfRangeException(nameof(n), $"n must be between 0 and {MaxFactorialN}.");

        return Factorials[n];
    }

    public int[] First(int n) => [.. Enumerable.Range(1, n)];

    public int[] GetByIndex(BigInteger index, int n)
    {
        if (index < 0 || index >= Factorial(n))
            throw new ArgumentOutOfRangeException(nameof(index));

        var available = Enumerable.Range(1, n).ToList();
        var result = new int[n];
        var remaining = index;

        for (int i = 0; i < n; i++)
        {
            var factorial = Factorial(n - i - 1);
            int position = (int)BigInteger.DivRem(remaining, factorial, out remaining);

            result[i] = available[position];
            available.RemoveAt(position);
        }

        return result;
    }

    public bool NextPermutation(int[] a)
    {
        int pivot = a.Length - 2;
        while (pivot >= 0 && a[pivot] >= a[pivot + 1])
            pivot--;

        if (pivot < 0)
            return false;

        int swap = a.Length - 1;
        while (a[swap] <= a[pivot])
            swap--;

        (a[pivot], a[swap]) = (a[swap], a[pivot]);
        Array.Reverse(a, pivot + 1, a.Length - pivot - 1);
        return true;
    }

    private static BigInteger[] BuildFactorials()
    {
        var table = new BigInteger[MaxFactorialN + 1];
        table[0] = BigInteger.One;
        for (int i = 1; i <= MaxFactorialN; i++)
            table[i] = table[i - 1] * i;
        return table;
    }
}
