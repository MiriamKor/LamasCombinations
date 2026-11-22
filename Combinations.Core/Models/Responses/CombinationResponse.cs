namespace Combinations.Models.Responses;

public class CombinationResponse
{
    public string? SerialNumber { get; set; }
    public List<int> Combination { get; set; } = new();
    public string? Message { get; set; }
}
