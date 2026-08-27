using FileProcessingService.Models;

namespace FileProcessingService.Services.Interfaces
{
    public interface ICsvProcessingService
    {
        Task<CsvProcessingResult> CalculateColumnAverageAsync(
            Stream stream,
            string? preferredColumn);
    }
}
