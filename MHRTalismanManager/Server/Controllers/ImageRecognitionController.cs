using System.Threading.Tasks;
using MHRTalismanManager.Server.Services;
using MHRTalismanManager.Shared;
using Microsoft.AspNetCore.Mvc;

namespace MHRTalismanManager.Server.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ImageRecognitionController : ControllerBase
    {
        private readonly DataExtractionService _dataExtractionService;

        public ImageRecognitionController(DataExtractionService dataExtractionService)
        {
            _dataExtractionService = dataExtractionService;
        }

        [HttpPost]
        public async Task<TalismanDto> ExtractDataFromImage()
        {
            await using var stream = Request.Body;
            return await _dataExtractionService.ExtractFromImage(stream);
        }
    }
}
