using FileProcessingService.Models;
using FileProcessingService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FileProcessingService.Controllers
{
    [ApiController]
    [Route("api/files")]
    public class FilesController : ControllerBase
    {
        private readonly ICsvProcessingService _csvProcessingService;
        private readonly IFileTrackingService _fileTrackingService;
        private readonly ILogger<FilesController> _logger;

        public FilesController(
            ICsvProcessingService csvProcessingService,
            IFileTrackingService fileTrackingService,
            ILogger<FilesController> logger)
        {
            _csvProcessingService = csvProcessingService;
            _fileTrackingService = fileTrackingService;
            _logger = logger;
        }

        [HttpPost("upload")]
        public async Task<ActionResult<ProcessingResultDto>> UploadFile(
            IFormFile file,
            [FromQuery] string? column)
        {
            if(file is null ||
                file.Length == 0)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Message = "No file was uploaded, or the file is empty."
                });
            }

            var stopwatch = Stopwatch.StartNew();

            try
            {
                using var stream = file.OpenReadStream();
                var result = await
                    _csvProcessingService.CalculateColumnAverageAsync(
                        stream,
                        column);

                stopwatch.Stop();

                var record = new FileProcessingRecord
                {
                    FileName = file.FileName,
                    FileSizeBytes = file.Length,
                    RowsProcessed = result.RowsProcessed,
                    ColumnAnalyzed = result.ColumnAnalyzed,
                    CalculatedAverage = result.Average,
                    ProcessingDurationMs = stopwatch.ElapsedMilliseconds,
                    Success = true
                };

                _fileTrackingService.RecordSuccess(record);

                return Ok(new ProcessingResultDto
                {
                    RecordId = record.Id,
                    FileName = record.FileName,
                    RowsProcessed = record.RowsProcessed,
                    ColumnAnalyzed = record.ColumnAnalyzed,
                    Average = record.CalculatedAverage
                });
            }
            catch(InvalidOperationException ex)
            {
                stopwatch.Stop();

                _logger.LogWarning(ex,
                    "Failed to process file {FileName}",
                    file.FileName);

                _fileTrackingService.RecordFailure(
                    file.FileName,
                    ex.Message,
                    stopwatch.ElapsedMilliseconds);

                return BadRequest(new ApiErrorResponse
                {
                    Message = "Failed to process the uploaded file.",
                    Detail = ex.Message
                });
            }
            catch(Exception ex)
            {
                stopwatch.Stop();

                _logger.LogError(ex,
                    "Unexpected error processing file {FileName}",
                    file.FileName);

                _fileTrackingService.RecordFailure(
                    file.FileName,
                    "Unexpected server error.",
                    stopwatch.ElapsedMilliseconds);

                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An unexpected error occurred while processing the file."
                });
            }
        }
    }
}
