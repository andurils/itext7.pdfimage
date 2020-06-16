using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System;
using System.Collections.Generic;
using System.IO;
using itext.pdfimage.Models;
using itext.pdfimage.Extensions;
using System.Threading;

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Diagnostics;

namespace itext.pdfimage
{
    public class PdfToImageConverter
    {
        private static int counter;

        public IEnumerable<Bitmap> ConvertToBitmaps(PdfDocument pdfDocument)
        {
            counter = 0;

            var numberOfPages = pdfDocument.GetNumberOfPages();

            for (var i = 1; i <= numberOfPages; i++)
            {
                var currentPage = pdfDocument.GetPage(i);

                yield return ConvertToBitmap(currentPage);
            }
        }

        public IEnumerable<Stream> ConvertToJpgStreams(PdfDocument pdfDocument)
        {
            foreach (var bmp in ConvertToBitmaps(pdfDocument))
            {
                using (var ms = new MemoryStream())
                {
                    bmp.Save(ms, ImageFormat.Jpeg);
                    yield return ms;
                }
                bmp.Dispose();
            }
        }

        public Bitmap ConvertToBitmap(PdfPage pdfPage)
        {
            var rotation = pdfPage.GetRotation();

            //var size = currentPage.GetPageSizeWithRotation();
            var size = pdfPage.GetPageSize();
            var width = size.GetWidth().PointsToPixels();
            var height = size.GetHeight().PointsToPixels();

            var chunkDictionairy = new SortedDictionary<float, IChunk>();

            FilteredEventListener listener = new FilteredEventListener();
            listener.AttachEventListener(new TextListener(chunkDictionairy, IncreaseCounter));
            listener.AttachEventListener(new ImageListener(chunkDictionairy, IncreaseCounter));
            listener.AttachEventListener(new PathListener(chunkDictionairy, IncreaseCounter, size.GetHeight()));
            PdfCanvasProcessor processor = new PdfCanvasProcessor(listener);
            processor.ProcessPageContent(pdfPage);

            ////var size = currentPage.GetPageSizeWithRotation();
            //var size = pdfPage.GetPageSize(); 
            //var width = size.GetWidth().PointsToPixels();
            //var height = size.GetHeight().PointsToPixels();

            Bitmap bmp = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.FillRectangle(Brushes.White, 0, 0, bmp.Width, bmp.Height);

                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;

                foreach (var chunk in chunkDictionairy)
                {
                    g.ResetTransform();

                    g.RotateTransform(-rotation);

                    if (chunk.Value is Models.ImageChunk imageChunk)
                    {
                        var imgW = imageChunk.W.PointsToPixels();
                        var imgH = imageChunk.H.PointsToPixels();
                        var imgX = imageChunk.X.PointsToPixels();
                        var imgY = (size.GetHeight() - imageChunk.Y - imageChunk.H).PointsToPixels();

                        g.TranslateTransform(imgX, imgY, MatrixOrder.Append);
                        g.DrawImage(imageChunk.Image, 0, 0, imgW, imgH);
                        imageChunk.Image.Dispose();
                    }
                    else if (chunk.Value is Models.TextChunk textChunk)
                    {

                        //textChunk.Rect.GetHeight
                        var chunkX = textChunk.Rect.GetX().PointsToPixels();
                        var chunkY = bmp.Height - textChunk.Rect.GetY().PointsToPixels();
                        var fontSize = (textChunk.FontSize * textChunk.TextZoom).PointsToPixels();

                        Font font;
                        try
                        {
                            font = new Font(textChunk.FontFamily, fontSize, textChunk.FontStyle, GraphicsUnit.Pixel);
                        }
                        catch (Exception ex)
                        {
                            //log error

                            font = new Font("Calibri", 12, textChunk.FontStyle, GraphicsUnit.Pixel);
                        }

                        g.TranslateTransform(chunkX, chunkY, MatrixOrder.Append);

                        //g.DrawString(textChunk.Text, font, new SolidBrush(textChunk.Color), chunkX, chunkY);
                        g.DrawString(textChunk.Text, font, new SolidBrush(textChunk.Color), 0, 0);
                    }
                    else if (chunk.Value is Models.PathChunk pathChunk)
                    {
                        Trace.WriteLine("pathChunk pathChunk pathChunk");
                        Pen newPen = new Pen(Color.Black);//定义一个画笔
                        float x1 = ((float)pathChunk.StartPath.x).PointsToPixels();
                        float y1 = ((float)pathChunk.StartPath.y).PointsToPixels();
                        //float y1 = pathChunk.StartPath.y < 0 ? bmp.Height + ((float)pathChunk.StartPath.y).PointsToPixels() : bmp.Height - ((float)pathChunk.StartPath.y).PointsToPixels();
                        float x2 = ((float)pathChunk.EndPath.x).PointsToPixels();
                        float y2 = ((float)pathChunk.EndPath.y).PointsToPixels();
                        //float y2 = pathChunk.EndPath.y < 0 ? bmp.Height + ((float)pathChunk.EndPath.y).PointsToPixels() : bmp.Height - ((float)pathChunk.EndPath.y).PointsToPixels();

                        g.DrawLine(newPen, x1, y1, x2, y2);//绘制直线
                        //g.DrawLine(newPen, ((float)pathChunk.StartPath.x).PointsToPixels(), bmp.Height - ((float)pathChunk.StartPath.y).PointsToPixels(), ((float)pathChunk.EndPath.x).PointsToPixels(), bmp.Height - ((float)pathChunk.EndPath.y).PointsToPixels());//绘制直线

                    }
                }

                g.Flush();
            }

            return bmp;
        }

        public Stream ConvertToJpgStream(PdfPage pdfPage)
        {
            var bmp = ConvertToBitmap(pdfPage);
            using (var ms = new MemoryStream())
            {
                bmp.Save(ms, ImageFormat.Jpeg);
                bmp.Dispose();
                return ms;
            }
        }

        private Func<float> IncreaseCounter = () => counter = Interlocked.Increment(ref counter);
    }
}
