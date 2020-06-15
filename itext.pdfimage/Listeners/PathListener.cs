using itext.pdfimage.Models;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System;
using System.Collections.Generic;
using System.Text;

namespace iText.Kernel.Pdf.Canvas.Parser.Listener
{


    public class PathListener : FilteredEventListener
    {
        private readonly SortedDictionary<float, IChunk> chunkDictionairy;
        private Func<float> increaseCounter;

        public float Height { get; private set; }

        public PathListener(SortedDictionary<float, IChunk> chunkDictionairy, Func<float> increaseCounter, float height)
        {
            this.chunkDictionairy = chunkDictionairy;
            this.increaseCounter = increaseCounter;
            this.Height = height;
        }

      
        public override void EventOccurred(IEventData data, EventType type)
        {
            List<PathChunk> paths = new List<PathChunk>();
            //是否读取到表格划块
            if (!type.Equals(EventType.RENDER_PATH)) return;

            PathRenderInfo renderInfo = (PathRenderInfo)data;

            var zoom = renderInfo.GetCtm().Get(0);
            //如果是假线 不处理
            int flag = renderInfo.GetOperation();
            if (flag == PathRenderInfo.NO_OP) return;


            var ctm = renderInfo.GetGraphicsState().GetCtm();

            var t = ctm.Get(6);
            var t1 = ctm.Get(7);

            var subpaths = renderInfo.GetPath().GetSubpaths();


            foreach (var path in subpaths)
            {

                var shapes = path.GetSegments();
                foreach (var shape in shapes)
                {
                    var basePoint = shape.GetBasePoints();  
                    paths.Add(new PathChunk(basePoint));
                }
            }


            //#region 缩放
            //if (zoom != 1)
            //{
            //    foreach (var path in subpaths)
            //    {
            //        var shapes = path.GetSegments();
            //        foreach (var shape in shapes)
            //        {
            //            var b = shape.GetBasePoints();
            //            foreach (var point in b)
            //            {
            //                point.x = point.x * zoom;
            //                point.y = point.y * zoom;
            //            }
            //        }
            //    }
            //}
            //#endregion

            ////如果是矩形 加上最后一条线
            //if (subpaths.Count == 2 && subpaths[0].GetSegments().Count == 3 && subpaths[1].GetSegments().Count == 0)
            //{
            //    var segList = subpaths[0].GetSegments();
            //    #region 全局线框不画
            //    if (segList[0].GetBasePoints()[0].x <= 0 && segList[0].GetBasePoints()[0].y <= 0)
            //    {
            //        return;
            //    }
            //    #endregion
            //    #region 画上底线
            //    //var basePoint = segList[2].GetBasePoints();
            //    List<Point> basePoint = new List<Point>();
            //    basePoint.Add(new Point(segList[2].GetBasePoints()[1]));
            //    basePoint.Add(new Point(subpaths[1].GetStartPoint()));
            //    basePoint[0].y = this.Height - basePoint[0].y;
            //    basePoint[1].y = this.Height - basePoint[1].y;
            //    paths.Add(new PathChunk(basePoint));
            //    #endregion
            //    #region 特殊矩形处理
            //    var xBasePoint = segList[0].GetBasePoints();
            //    var yBasePoint = segList[1].GetBasePoints();
            //    var xSeBasePoint = segList[2].GetBasePoints();
            //    var ttttt = renderInfo.GetGraphicsState();
            //    #endregion

            //    foreach (var path in subpaths)
            //    {

            //        var shapes = path.GetSegments();
            //        foreach (var shape in shapes)
            //        {
            //            var b = shape.GetBasePoints();
            //            foreach (var point in b)
            //            {

            //                if (t1 == 0)
            //                {
            //                    point.y = this.Height - point.y;
            //                }
            //                if (point.y > 768.199996948242 && point.y < 770.199996948242)
            //                { }
            //            }
            //            paths.Add(new PathChunk(b));
            //        }
            //    }
            //    //return; 
            //}
            ////如果是画线
            //if (t1 == 0)
            //{
            //    t1 = this.Height;
            //}
            //foreach (var path in subpaths)
            //{

            //    var shapes = path.GetSegments();
            //    foreach (var shape in shapes)
            //    {
            //        var basePoint = shape.GetBasePoints();
            //        foreach (var point in basePoint)
            //        {

            //            point.x = point.x + t;
            //            point.y = this.Height - t1 + point.y;
            //        }

            //        paths.Add(new PathChunk(basePoint));
            //    }
            //}

            foreach (var item in paths)
            {
                float counter = increaseCounter();
                chunkDictionairy.Add(counter, item);
            }
            base.EventOccurred(data, type);

        }

    }

}
