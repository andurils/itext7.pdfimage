using iText.Kernel.Font;
using iText.Kernel.Geom;
using System;
using System.Collections.Generic;
using System.Text;

namespace itext.pdfimage.Models
{
    public enum 线方向 { 横向, 纵向, 圆点 }

    public class PathChunk : IChunk
    {
        public PDFPoint StartPath { get; set; }
        public PDFPoint EndPath { get; set; }
        //public PDFTable PDFTable { get; set; }
        public bool IsDeleted { get; set; }

        public PathChunk(double x1, double y1, double x2, double y2)
        {
            StartPath = new PDFPoint(new Point(x1, y1));
            EndPath = new PDFPoint(new Point(x2, y2));
            IsDeleted = false;
            Init();
        }
        public PathChunk(IList<Point> pathRenderInfo)
        {
            this.StartPath = new PDFPoint(pathRenderInfo[0]);
            this.EndPath = new PDFPoint(pathRenderInfo[1]);
            IsDeleted = false;
            Init();
        }

        private void Init()
        {
            if ((int)(this.EndPath.y) == 530 && (int)(this.EndPath.x) == 463)
            { }
            //确保所有横向线都是从左到右
            if (this.EndPath.x < this.StartPath.x)
            {
                var temp = this.EndPath;
                this.EndPath = this.StartPath;
                this.StartPath = temp;
            }
            //确保所有纵向线都是从上往下
            if (this.EndPath.y < this.StartPath.y)
            {
                var temp = this.EndPath;
                this.EndPath = this.StartPath;
                this.StartPath = temp;
            }

        }
        public 线方向 Direction
        {
            get
            {
                if (StartPath.x == EndPath.x && StartPath.y == EndPath.y)
                {
                    return 线方向.圆点;
                }
                else if (StartPath.y == EndPath.y)
                {
                    return 线方向.横向;
                }
                else if (StartPath.x == EndPath.x)
                {
                    return 线方向.纵向;

                }
                return 线方向.圆点;
            }
        }
    }
}
