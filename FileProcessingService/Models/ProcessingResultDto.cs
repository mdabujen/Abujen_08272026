namespace FileProcessingService.Models
{
    public class ProcessingResultDto
    {
        public Guid RecordId { get; init; }
        public string FileName { get; init; } = string.Empty;
        public int RowsProcessed { get; init; }
        public string ColumnAnalyzed { get; init; } = string.Empty;
        public double Average { get; init; }
    }
}