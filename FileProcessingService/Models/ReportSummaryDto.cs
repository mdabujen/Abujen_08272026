namespace FileProcessingService.Models
{
    public class ReportSummaryDto
    {
        public int TotalFilesProcessed { get; init; }
        public int SuccessfulFiles { get; init; }
        public int FailedFiles { get; init; }
        public long TotalRowsProcessed { get; init; }
        public DateTime? LastProcessedAtUtc { get; init; }
    }
}