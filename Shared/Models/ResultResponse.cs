namespace Shared.Models;

public class ResultResponse
{
    public bool Succeeded { get; set; }
    public string? Message { get; set; }
    public object? Content { get; set; }
}
