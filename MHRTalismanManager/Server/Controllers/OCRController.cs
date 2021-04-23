using System.Threading.Tasks;
using MHRTalismanManager.Server.Services;
using MHRTalismanManager.Shared;
using Microsoft.AspNetCore.Mvc;

namespace MHRTalismanManager.Server.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class OcrController : ControllerBase
    {
        private readonly DataExtractionService _dataExtractionService;

        public OcrController(DataExtractionService dataExtractionService)
        {
            _dataExtractionService = dataExtractionService;
        }

        [HttpPost]
        public async Task<TalismanDto> DoOcr()
        {
            await using var stream = Request.Body;
            return await _dataExtractionService.ExtractFromImage(stream);
        }
    }
}
