namespace Combinations.Services;

public class NotStartedException()
    : InvalidOperationException("Not started. Call start first.");
