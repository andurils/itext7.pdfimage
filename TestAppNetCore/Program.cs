using iText.Kernel.Pdf;
using itext.pdfimage.Extensions;
using System;
using System.IO;
using System.Drawing.Imaging;
using System.Collections.Generic;

namespace TestAppNetCore
{
    class Program
    {
        static void Main(string[] args)
        {
            
            var testList = new Dictionary<string, int>();
            testList.Add(@"D:\attachment\data\1eb7ab119371c1337afb14c63c33df85\P020190524348747199779.pdf", 25);
            testList.Add(@"D:\attachment\data\0944012e8e050a147562483a70f285fb\P020190923591889171962.pdf", 105);

            foreach (var dict in testList)
            {
                Console.WriteLine("Bliep");
                var filePath = dict.Key;
                var pdf = File.Open(filePath, FileMode.Open);

                //var pdf = File.Open(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "wave.pdf"), FileMode.Open);

                var reader = new PdfReader(pdf);
                var pdfDocument = new PdfDocument(reader);
                //var bitmaps = pdfDocument.ConvertToBitmaps();

                //foreach (var bitmap in bitmaps)
                //{
                //    bitmap.Save(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), $"wave-{DateTime.Now.Ticks}.png"), ImageFormat.Png);
                //    bitmap.Dispose();
                //}

                var page1 = pdfDocument.GetPage(dict.Value);
                var bitmap1 = page1.ConvertPageToBitmap();
                //bitmap1.Save(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), $"wave-page1-{DateTime.Now.Ticks}.png"), ImageFormat.Png);
                bitmap1.Save(Path.Combine("test", $"{DateTime.Now.Ticks}.png"), ImageFormat.Png);
                bitmap1.Dispose();

                Console.WriteLine("Bliep!");
            }
            
        }
    }
}
