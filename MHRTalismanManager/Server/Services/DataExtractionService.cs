using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MHRTalismanManager.Shared;
using MoreLinq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Filters;
using Tesseract;
using Image = SixLabors.ImageSharp.Image;
using Rectangle = SixLabors.ImageSharp.Rectangle;
using Size = SixLabors.ImageSharp.Size;

namespace MHRTalismanManager.Server.Services
{
    public class DataExtractionService
    {
        private const int Point1 = 48;
        private const int Point2 = 64;
        private const int Point3 = 80;
        private const int Point4 = 96;
        private const int Point5 = 112;
        private const int Point6 = 128;
        private const int Point7 = 144;

        private readonly Dictionary<string, string> _corrections = new()
                                                                   {
                                                                       { "\u2014", "-" },
                                                                   };

        private readonly TesseractEngine _engine;

        public DataExtractionService(TesseractEngine engine)
        {
            _engine = engine;
        }

        public async Task<TalismanDto> ExtractFromImage(Stream stream)
        {
            var talisman = new TalismanDto();
            using var image = await Image.LoadAsync<Rgba32>(stream);

            var meldingText = await GetText(_engine, image, new Rectangle(425, 102, 169, 23), new Size(700, 50));

            var (operation, infoRectangle) = meldingText switch
            {
                "Melding Result" =>
                    (TalismanOperation.Add, new Rectangle(745, 102, 249, 528)),
                "Equipment Box" =>
                    (TalismanOperation.Remove, new Rectangle(672, 102, 249, 528)),
                _ => (TalismanOperation.Add, new Rectangle(1008, 90, 249, 528)),
            };
            talisman.Operation = operation;
            image.Mutate(c => c.Crop(infoRectangle));

            //28 between slots
            //129 height of slot triangle
            //165 left triangle
            //172 middle triangle
            //179 right triangle
            //131 height of testpixel

            var hasSlot1 = false;
            var hasSlot2 = false;
            var hasSlot3 = false;
            var slot1Points = 0;
            var slot2Points = 0;
            var slot3Points = 0;
            image.Mutate(c => c.ProcessPixelRowsAsVector4((span, point) =>
                                                          {
                                                              var (_, y) = point;
                                                              if (y is not (213 or 264 or 129 or 131))
                                                                  return;

                                                              if (y is (213 or 264))
                                                              {
                                                                  var pixels = new List<Vector4> { span[Point1], span[Point2], span[Point3], span[Point4], span[Point5], span[Point6], span[Point7] };
                                                                  var points = pixels.Count(p => new Rgba32(p).B > 200);

                                                                  switch (y)
                                                                  {
                                                                      case 213:
                                                                          talisman.Skill1.Points = points;
                                                                          break;
                                                                      case 264 when points > 0:
                                                                          talisman.Skill2.Points = points;
                                                                          break;
                                                                  }
                                                              }
                                                              else if (y is 131)
                                                              {
                                                                  hasSlot1 = HasSlot(span[172]);
                                                                  hasSlot2 = HasSlot(span[200]);
                                                                  hasSlot3 = HasSlot(span[228]);
                                                              }
                                                              else
                                                              {
                                                                  slot1Points = new[] { span[165], span[172], span[179] }.Select(p => new Rgba32(p))
                                                                                                                         .Count(p => p.R <= 90 && p.G <= 90 && p.B <= 90);
                                                                  slot2Points = new[] { span[193], span[200], span[207] }.Select(p => new Rgba32(p))
                                                                                                                         .Count(p => p.R <= 90 && p.G <= 90 && p.B <= 90);
                                                                  slot3Points = new[] { span[221], span[228], span[235] }.Select(p => new Rgba32(p))
                                                                                                                         .Count(p => p.R <= 90 && p.G <= 90 && p.B <= 90);
                                                              }
                                                          }));

            talisman.Skill1.Name = await GetText(_engine, image, new Rectangle(26, 178, 216, 18), new Size(700, 50));

            //await image.SaveAsync(_env.ContentRootFileProvider.GetFileInfo(Guid.NewGuid() + ".jpg")
            //                          .PhysicalPath);
            talisman.Skill2.Name = talisman.Skill2.Points.HasValue
                                       ? await GetText(_engine, image, new Rectangle(26, 229, 216, 18), new Size(700, 50))
                                       : string.Empty;

            if (hasSlot1 && slot1Points == 0 || hasSlot2 && slot2Points == 0 || hasSlot3 && slot3Points == 0)
                //decoration detected, ignoring this talisman
                talisman.Operation = TalismanOperation.Ignore;

            if (hasSlot1)
                talisman.Slot1 = (SlotType)slot1Points;
            if (hasSlot1 && hasSlot2)
                talisman.Slot2 = (SlotType)slot2Points;
            if (hasSlot3 && hasSlot2 && hasSlot1)
                talisman.Slot3 = (SlotType)slot3Points;

            return talisman;
        }

        private bool HasSlot(Vector4 testPixel)
        {
            var testColor = new Rgba32(testPixel);
            var testData = new[] { testColor.R, testColor.G, testColor.B };

            return testData.All(x => x > 100);
        }

        private async Task<string> GetText(TesseractEngine engine, Image<Rgba32> image, Rectangle cropRectangle, Size resizeSize, int invertAmount = 1)
        {
            using var skillText = image.Clone();
            skillText.Mutate(c => c.Crop(cropRectangle)
                                   .ApplyProcessor(new InvertProcessor(invertAmount))
                                   .Resize(resizeSize));

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
