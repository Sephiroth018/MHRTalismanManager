using System;
using System.IO;
using MHRTalismanManager.Server.Services;
using Snapshooter;
using Snapshooter.Xunit;
using Tesseract;
using Xunit;

namespace MHRTalismanManager.Server.Tests
{
    public class DataExtractionServiceTest
    {
        public class ExtractFromImage : IDisposable
        {
            private readonly TesseractEngine _engine;

            public ExtractFromImage()
            {
                _engine = new TesseractEngine(new DirectoryInfo("tesseract").FullName, "eng", EngineMode.Default);
            }

            public void Dispose()
            {
                _engine.Dispose();
            }

            [Theory]
            [InlineData("guildhall-meldingresult-oneskill")]
            [InlineData("guildhall-meldingresult-twoskill")]
            [InlineData("guildhall-meldingresult-0-0-0")]
            [InlineData("guildhall-meldingresult-1-0-0")]
            [InlineData("guildhall-meldingresult-1-1-0")]
            //[InlineData("steelplaza-meldingresult-1-1-1")]
            [InlineData("guildhall-meldingresult-2-0-0")]
            [InlineData("guildhall-meldingresult-2-1-0")]
            //[InlineData("steelplaza-meldingresult-2-2-0")]
            //[InlineData("steelplaza-meldingresult-2-1-1")]
            //[InlineData("steelplaza-meldingresult-2-2-1")]
            //[InlineData("steelplaza-meldingresult-2-2-2")]
            //[InlineData("steelplaza-meldingresult-3-0-0")]
            //[InlineData("steelplaza-meldingresult-3-1-0")]
            //[InlineData("steelplaza-meldingresult-3-2-0")]
            //[InlineData("steelplaza-meldingresult-3-3-0")]
            //[InlineData("steelplaza-meldingresult-3-3-1")]
            //[InlineData("steelplaza-meldingresult-3-3-2")]
            //[InlineData("steelplaza-meldingresult-3-3-3")]
            //[InlineData("steelplaza-meld-oneskill")]
            //[InlineData("steelplaza-meld-twoskill")]
            //[InlineData("steelplaza-meld-0-0-0")]
            //[InlineData("steelplaza-meld-1-0-0")]
            //[InlineData("steelplaza-meld-1-1-0")]
            //[InlineData("steelplaza-meld-1-1-1")]
            //[InlineData("steelplaza-meld-2-0-0")]
            //[InlineData("steelplaza-meld-2-1-0")]
            //[InlineData("steelplaza-meld-2-2-0")]
            //[InlineData("steelplaza-meld-2-1-1")]
            //[InlineData("steelplaza-meld-2-2-1")]
            //[InlineData("steelplaza-meld-2-2-2")]
            //[InlineData("steelplaza-meld-3-0-0")]
            //[InlineData("steelplaza-meld-3-1-0")]
            [InlineData("guildhall-meld-3-2-0")]
            //[InlineData("steelplaza-meld-3-3-0")]
            //[InlineData("steelplaza-meld-3-3-1")]
            //[InlineData("steelplaza-meld-3-3-2")]
            //[InlineData("steelplaza-meld-3-3-3")]
            [InlineData("guildhall-meld-deco1")]
            //[InlineData("steelplaza-meld-deco2")]
            //[InlineData("steelplaza-meld-deco3")]
            //[InlineData("steelplaza-box-oneskill")]
            //[InlineData("steelplaza-box-twoskill")]
            //[InlineData("steelplaza-box-0-0-0")]
            //[InlineData("steelplaza-box-1-0-0")]
            //[InlineData("steelplaza-box-1-1-0")]
            //[InlineData("steelplaza-box-1-1-1")]
            //[InlineData("steelplaza-box-2-0-0")]
            //[InlineData("steelplaza-box-2-1-0")]
            //[InlineData("steelplaza-box-2-2-0")]
            //[InlineData("steelplaza-box-2-1-1")]
            //[InlineData("steelplaza-box-2-2-1")]
            //[InlineData("steelplaza-box-2-2-2")]
            //[InlineData("steelplaza-box-3-0-0")]
            //[InlineData("steelplaza-box-3-1-0")]
            //[InlineData("steelplaza-box-3-2-0")]
            //[InlineData("steelplaza-box-3-3-0")]
            //[InlineData("steelplaza-box-3-3-1")]
            //[InlineData("steelplaza-box-3-3-2")]
            //[InlineData("steelplaza-box-3-3-3")]
            [InlineData("steelplaza-box-deco1")]
            //[InlineData("steelplaza-box-deco2")]
            //[InlineData("steelplaza-box-deco3")]
            public async void ShouldExtractTalismanData(string filename)
            {
                var stream = new FileInfo(Path.Combine("testimages", $"{filename}.jpg")).OpenRead();
                var service = new DataExtractionService(_engine);

                var talisman = await service.ExtractFromImage(stream);

                Snapshot.Match(talisman, SnapshotNameExtension.Create(filename));
            }
        }
    }
}
