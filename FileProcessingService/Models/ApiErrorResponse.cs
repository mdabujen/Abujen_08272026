namespace FileProcessingService.Models
{
    public class ApiErrorResponse
    {
        public string Message { get; init; } = string.Empty;
        public string? Detail { get; init; }
    }
}
