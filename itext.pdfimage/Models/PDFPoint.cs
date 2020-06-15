using iText.Kernel.Geom;
using System;
using System.Collections.Generic;
using System.Text;

namespace itext.pdfimage.Models
{




    public class PDFPoint : Point
    {
        public PDFPoint(Point p)
        {
            this.x = p.x;
            this.y = p.y;
        }
        public PDFPoint RightPoint { get; set; }
        public PDFPoint DownPoint { get; set; }


    }
}
