using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MHRTalismanManager.Shared;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using MoreLinq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Filters;
using Tesseract;
using Image = SixLabors.ImageSharp.Image;
using Rectangle = SixLabors.ImageSharp.Rectangle;

namespace MHRTalismanManager.Server.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class OCRController : ControllerBase
    {
        private const int Point1 = 789;
        private const int Point2 = 805;
        private const int Point3 = 821;
        private const int Point4 = 837;
        private const int Point5 = 853;
        private const int Point6 = 869;
        private const int Point7 = 885;

        private readonly Dictionary<string, string> _corrections = new()
                                                                   {
                                                                       { "\u2014", "-" },
                                                                   };

        private readonly IWebHostEnvironment _env;

        public OCRController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost]
        public async Task<Talisman> DoOcr()
        {
            var talisman = new Talisman();
            await using var stream = Request.Body;
            using var image = await Image.LoadAsync<Rgba32>(stream);

            image.Mutate(c => c.ProcessPixelRowsAsVector4((span, point) =>
                                                          {
                                                              var (_, y) = point;
                                                              if (y is not (316 or 367))
                                                                  return;

                                                              var pixels = new List<Vector4> { span[Point1], span[Point2], span[Point3], span[Point4], span[Point5], span[Point6], span[Point7] };
                                                              var points = pixels.Count(p => new Rgba32(p).B > 200);

                                                              switch (y)
                                                              {
                                                                  case 316:
                                                                      talisman.Skill1.Points = points;
                                                                      break;
                                                                  case 367 when points > 0:
                                                                      talisman.Skill2.Points = points;
                                                                      break;
                                                              }
                                                          }));

            using var engine = new TesseractEngine(_env.ContentRootFileProvider.GetFileInfo("tesseract")
                                                       .PhysicalPath,
                                                   "eng",
                                                   EngineMode.Default);

            talisman.Skill1.Name = await DoSkillNameOcr(engine, image, 280);
            talisman.Skill2.Name = talisman.Skill2.Points.HasValue
                                       ? await DoSkillNameOcr(engine, image, 331)
                                       : string.Empty;

            return talisman;
        }

        private async Task<string> DoSkillNameOcr(TesseractEngine engine, Image<Rgba32> image, int line)
        {
            using var skillText = image.Clone();
            skillText.Mutate(c => c.Crop(new Rectangle(772, line, 216, 18))
                                   .ApplyProcessor(new InvertProcessor(1))
                                   .Resize(700, 50));

            await using var skillStream = new MemoryStream();
            await skillText.SaveAsJpegAsync(skillStream);
            skillStream.Position = 0;
            using var img = new Bitmap(System.Drawing.Image.FromStream(skillStream));
            using var page = engine.Process(img, PageSegMode.SingleLine);
            var resultText = Regex.Unescape(page.GetText()
                                                .Trim()
                                                .TrimEnd('\n'));
            _corrections.ForEach(c => resultText = resultText.Replace(c.Key, c.Value));

            return resultText;
        }
    }
}
