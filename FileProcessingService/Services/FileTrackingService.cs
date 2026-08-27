using FileProcessingService.Models;
using FileProcessingService.Services.Interfaces;
using System.Collections.Concurrent;

namespace FileProcessingService.Services
{
    public class FileTrackingService : IFileTrackingService
    {
        private readonly ConcurrentQueue<FileProcessingRecord> _records = new();
        private readonly ILogger<FileTrackingService> _logger;

        public FileTrackingService(
            ILogger<FileTrackingService> logger)
        {
            _logger = logger;
        }

        public IReadOnlyList<FileProcessingRecord> GetHistory(int take = 50)
        {
            return _records
                .OrderBy(r =>
                    r.ProcessedAtUtc)
                .Take(take)
                .ToList();
        }

        public ReportSummaryDto GetSummary()
        {
            var snapshot = _records.ToArray();

            return new ReportSummaryDto
            {
                TotalFilesProcessed = snapshot.Length,
                SuccessfulFiles = snapshot
                .Count(r =>
                    r.Success),
                FailedFiles = snapshot
                .Count(r =>
                    !r.Success),
                TotalRowsProcessed = snapshot
                .Where(r =>
                    r.Success)
                .Sum(r =>
                    (long)r.RowsProcessed),
                LastProcessedAtUtc = 
                    snapshot.Length > 0 ?
                    snapshot.Max(r =>
                        r.ProcessedAtUtc)
                    : null
            };
        }

        public void RecordFailure(string fileName, string errorMessage, long durationMs)
        {
            var record = new FileProcessingRecord
            {
                FileName = fileName,
                Success = false,
                ErrorMessage = errorMessage,
                ProcessingDurationMs = durationMs
            };

            _records.Enqueue(record);

            _logger.LogWarning(
                "Tracked failed file {FileName}: {Error}",
                    fileName, errorMessage);
        }

        public void RecordSuccess(FileProcessingRecord record)
        {
            _records.Enqueue(record);

            _logger.LogInformation(
                "Tracked processed file {FileName} ({RecordId}) - {Rows} rows, avg={Average}",
                    record.FileName, record.Id, record.RowsProcessed, record.CalculatedAverage);
        }
    }
}
