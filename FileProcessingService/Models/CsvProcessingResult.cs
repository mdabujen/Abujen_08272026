namespace FileProcessingService.Models
{
    public class CsvProcessingResult
    {
        public int RowsProcessed { get; init; }
        public string ColumnAnalyzed { get; init; } = string.Empty;
        public double Average { get; init; }
    }
}
