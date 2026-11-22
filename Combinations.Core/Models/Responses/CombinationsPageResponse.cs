namespace Combinations.Models.Responses;

public class CombinationsPageResponse
{
    public List<CombinationResponse> Combinations { get; set; } = new();
    public string TotalCombinations { get; set; } = "0";
    public string CurrentPage { get; set; } = "0";
    public string TotalPages { get; set; } = "0";
    public int PageSize { get; set; }
    public string? Message { get; set; }
}
