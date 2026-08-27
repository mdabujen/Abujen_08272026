namespace FileProcessingService.Models
{
    public class FileProcessingRecord
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string FileName { get; init; } = string.Empty;
        public int RowsProcessed { get; init; }
        public string ColumnAnalyzed { get; init; } = string.Empty;
        public double CalculatedAverage { get; init; }
        public DateTime ProcessedAtUtc { get; init; } = DateTime.UtcNow;
        public long ProcessingDurationMs { get; init; }
        public bool Success { get; init; }
        public string? ErrorMessage { get; init; }
    }
}