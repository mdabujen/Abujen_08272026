using FileProcessingService.Models;
using FileProcessingService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FileProcessingService.Controllers
{
    [ApiController]
    [Route("api/reports")]
    public class ReportsController : ControllerBase
    {
        private readonly IFileTrackingService _fileTrackingService;

        public ReportsController(IFileTrackingService fileTrackingService)
        {
            _fileTrackingService = fileTrackingService;
        }

        [HttpGet("summary")]
        public ActionResult<ReportSummaryDto> GetSummary()
        {
            var summary = _fileTrackingService.GetSummary();
            return Ok(summary);
        }

        [HttpGet("history")]
        public ActionResult<IReadOnlyList<FileProcessingRecord>> GetHistory(
            [FromQuery] int take = 50)
        {
            if(take <= 0 || take > 500)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Message = "The 'take' parameter must be between 1 and 500."
                });                
            }

            var history = _fileTrackingService.GetHistory(take);
            return Ok(history);
        }
    }
}
