using FileProcessingService.Models;

namespace FileProcessingService.Services.Interfaces
{
    public interface IFileTrackingService
    {
        void RecordSuccess(FileProcessingRecord record);
        void RecordFailure(
            string fileName,
            string errorMessage,
            long durationMs);
        ReportSummaryDto GetSummary();
        IReadOnlyList<FileProcessingRecord> GetHistory(int take = 50);
    }
}
