using itext.pdfimage.Models;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System;
using System.Collections.Generic;
using System.Linq;
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

            //是否读取到表格划块
            if (!type.Equals(EventType.RENDER_PATH) && !type.Equals(EventType.CLIP_PATH_CHANGED)) return;
            List<PathChunk> paths = new List<PathChunk>();
            if (type == EventType.RENDER_PATH)
            {
                PathRenderInfo renderInfo = (PathRenderInfo)data;

                var zoom = renderInfo.GetCtm().Get(0);
                //如果是假线 不处理
                int flag = renderInfo.GetOperation();
                if (flag == PathRenderInfo.NO_OP) return;


                var ctm = renderInfo.GetGraphicsState().GetCtm();

                var t = ctm.Get(6);
                var t1 = ctm.Get(7);

                var subpaths = renderInfo.GetPath().GetSubpaths();


                //foreach (var path in subpaths)
                //{

                //    var shapes = path.GetSegments();
                //    foreach (var shape in shapes)
                //    {
                //        var basePoint = shape.GetBasePoints();  
                //        paths.Add(new PathChunk(basePoint));
                //    }
                //}


                #region 缩放
                if (zoom != 1)
                {
                    foreach (var path in subpaths)
                    {
                        var shapes = path.GetSegments();
                        foreach (var shape in shapes)
                        {
                            var b = shape.GetBasePoints();
                            foreach (var point in b)
                            {
                                point.x = point.x * zoom;
                                point.y = point.y * zoom;
                            }
                        }
                    }
                }
                #endregion

                //如果是矩形 加上最后一条线
                if (subpaths.Count == 2 && subpaths[0].GetSegments().Count == 3 && subpaths[1].GetSegments().Count == 0)
                {
                    var segList = subpaths[0].GetSegments();


                    #region 全局线框不画
                    if (segList[0].GetBasePoints()[0].x <= 0 && segList[0].GetBasePoints()[0].y <= 0)
                    {
                        return;
                    }
                    #endregion
                    #region 画上底线
                    //var basePoint = segList[2].GetBasePoints();
                    List<Point> basePoint = new List<Point>();
                    basePoint.Add(new Point(segList[2].GetBasePoints()[1]));
                    basePoint.Add(new Point(subpaths[1].GetStartPoint()));
                    basePoint[0].y = this.Height - basePoint[0].y;
                    basePoint[1].y = this.Height - basePoint[1].y;
                    paths.Add(new PathChunk(basePoint));
                    #endregion
                    #region 特殊矩形处理
                    //var xBasePoint = segList[0].GetBasePoints();
                    //var yBasePoint = segList[1].GetBasePoints();
                    //var xSeBasePoint = segList[2].GetBasePoints();
                    //var ttttt = renderInfo.GetGraphicsState(); 
                    #endregion

                    foreach (var path in subpaths)
                    {
                        var shapes = path.GetSegments();
                        foreach (var shape in shapes)
                        {
                            var b = shape.GetBasePoints();
                            foreach (var point in b)
                            {

                                if (t1 == 0)
                                {
                                    point.y = this.Height - point.y;
                                }
                                if (point.y > 768.199996948242 && point.y < 770.199996948242)
                                { }
                            }
                            paths.Add(new PathChunk(b));
                        }
                    }
                }
                else
                {
                    //如果是画线
                    if (t1 == 0)
                    {
                        t1 = this.Height;
                    }
                    foreach (var path in subpaths)
                    {
                        var shapes = path.GetSegments();
                        foreach (var shape in shapes)
                        {
                            var basePoint = shape.GetBasePoints();
                            foreach (var point in basePoint)
                            {

                                point.x = point.x + t;
                                point.y = this.Height - t1 + point.y;
                            }
                            paths.Add(new PathChunk(basePoint));
                        }
                    }
                }
            }
            if (type == EventType.CLIP_PATH_CHANGED)
            {
                ClippingPathInfo renderInfo = (ClippingPathInfo)data;
                var s = renderInfo.GetGraphicsState().GetCtm();

                var t = s.Get(6);
                var t1 = s.Get(7);
                var clpath = renderInfo.GetClippingPath();
                var currentPoint = clpath.GetCurrentPoint();
                if (currentPoint != null && currentPoint.x == 0 && currentPoint.y == 0)
                {
                    return;
                }
                var subpaths = clpath.GetSubpaths();
                //如果是矩形裁剪
                if (subpaths.Count == 2 && subpaths[0].GetSegments().Count == 3 && subpaths[1].GetSegments().Count == 0)
                {
                    var isJISUAN = clpath.GetCurrentPoint().y == subpaths[0].GetStartPoint().y;
                    var segList = subpaths[0].GetSegments();
                    #region 修正坐标系
                    foreach (var path in subpaths)
                    {
                        var shapes = path.GetSegments();
                        foreach (var shape in shapes)
                        {
                            var b = shape.GetBasePoints();
                            foreach (var point in b)
                            {
                                if (t1 == 0 && isJISUAN)
                                {
                                    point.y = this.Height - point.y;
                                }
                            }
                        }
                    }
                    #endregion
                    #region 画上底线
                    List<Point> basePoint = new List<Point>();
                    basePoint.Add(new Point(segList[2].GetBasePoints()[1]));
                    basePoint.Add(new Point(segList[0].GetBasePoints()[0]));
                    //basePoint[1].y = this.Height - basePoint[1].y;

                    #endregion
                    #region 特殊矩形处理
                    var clipPaths = new List<PathChunk>(4);
                    var x1 = new PathChunk(segList[2].GetBasePoints());
                    var x2 = new PathChunk(segList[0].GetBasePoints());
                    var y1 = new PathChunk(basePoint);
                    var y2 = new PathChunk(segList[1].GetBasePoints());

                    clipPaths.Add(x1);
                    clipPaths.Add(x2);
                    clipPaths.Add(y1);
                    clipPaths.Add(y2);
                    var clipXpaths = clipPaths.Where(p => p.Direction == 线方向.横向).OrderBy(p => p.StartPath.y).ToList();
                    var clipYpaths = clipPaths.Where(p => p.Direction == 线方向.纵向).OrderBy(p => p.StartPath.x).ToList();
                    if (clipXpaths.Count != 2 || clipYpaths.Count != 2)
                    {
                        return;
                    }
                    if ((clipXpaths[1].EndPath.x - clipXpaths[1].StartPath.x) > clipXpaths[1].StartPath.x * 2
                        && (clipYpaths[1].EndPath.y - clipYpaths[1].StartPath.y) > clipYpaths[1].StartPath.y * 2
                        && (clipYpaths[1].EndPath.y - clipYpaths[1].StartPath.y + clipYpaths[1].StartPath.y * 2) >= this.Height - 50)
                    {
                        return;
                    }
                    for (int i = paths.Count < 4 ? 0 : paths.Count - 4; i < paths.Count && paths.Count > 0; i++)
                    {
                        var path = paths[i];
                        bool flag = false;
                        //删除重叠矩形的线
                        foreach (var xpath in clipPaths)
                        {
                            if (xpath.StartPath.x + 0.1 > path.StartPath.x && xpath.StartPath.x - 0.1 < path.StartPath.x
                                && xpath.EndPath.x + 0.1 > path.EndPath.x && xpath.EndPath.x - 0.1 < path.EndPath.x
                                && xpath.StartPath.y + 0.1 > path.StartPath.y && xpath.StartPath.y - 0.1 < path.StartPath.y
                                && xpath.EndPath.y + 0.1 > path.EndPath.y && xpath.EndPath.y - 0.1 < path.EndPath.y)
                            {
                                path.IsDeleted = true;
                                flag = true;
                                break;
                            }
                        }
                        //continue;
                        if (flag)
                        {
                            continue;
                        }
                        if (path.Direction == 线方向.横向
                            && path.StartPath.y + 0.001 > clipXpaths[0].StartPath.y && path.StartPath.y < clipXpaths[1].StartPath.y + 0.001)
                        {
                            if (clipXpaths[0].StartPath.x - path.StartPath.x > 0.001 && clipXpaths[0].EndPath.x - path.EndPath.x > -0.001
                                && clipXpaths[0].StartPath.x - 0.001 < path.EndPath.x)
                            {
                                path.EndPath.x = clipXpaths[0].StartPath.x;
                            }
                            else if (path.StartPath.x - clipXpaths[0].StartPath.x > 0.001 && path.EndPath.x - clipXpaths[0].EndPath.x > 0.001
                                 && path.StartPath.x + 0.001 < clipXpaths[0].EndPath.x)
                            {
                                path.StartPath.x = clipXpaths[0].EndPath.x;
                            }
                            else if (path.StartPath.x + 0.001 < clipXpaths[0].StartPath.x && path.EndPath.x - 0.001 > clipXpaths[0].EndPath.x)
                            {
                                paths.Add(new PathChunk(path.StartPath.x, path.StartPath.y, clipXpaths[0].StartPath.x, path.StartPath.y));
                                paths.Add(new PathChunk(clipXpaths[0].EndPath.x, path.EndPath.y, path.EndPath.x, path.EndPath.y));

                                path.IsDeleted = true;
                            }
                            else if (path.StartPath.x + 0.001 > clipXpaths[0].StartPath.x && path.EndPath.x - 0.001 < clipXpaths[0].EndPath.x)
                            {
                                path.IsDeleted = true;
                            }
                        }
                        if (path.Direction == 线方向.纵向
                            && path.StartPath.x + 0.001 > clipYpaths[0].StartPath.x && path.StartPath.x < clipYpaths[1].StartPath.x + 0.001)
                        {
                            if (clipYpaths[0].StartPath.y + 0.001 > path.StartPath.y && clipYpaths[0].EndPath.y > path.EndPath.y - 0.001
                                && clipYpaths[0].StartPath.y - 0.001 < path.EndPath.y)
                            {
                                path.EndPath.y = clipYpaths[0].StartPath.y;
                            }
                            else if (path.EndPath.y + 0.001 > clipYpaths[0].EndPath.y && path.StartPath.y > clipYpaths[0].StartPath.y - 0.001
                                && path.StartPath.y - 0.001 < clipYpaths[0].EndPath.y)
                            {
                                path.StartPath.y = clipYpaths[0].EndPath.y;
                            }
                            else if (path.StartPath.y - 0.001 < clipYpaths[0].StartPath.y && path.EndPath.y + 0.001 > clipYpaths[0].EndPath.y)
                            {
                                paths.Add(new PathChunk(path.StartPath.x, path.StartPath.y, path.StartPath.x, clipYpaths[0].StartPath.y));
                                paths.Add(new PathChunk(path.EndPath.x, clipYpaths[0].EndPath.y, path.EndPath.x, path.EndPath.y));

                                path.IsDeleted = true;
                            }
                            else if (path.StartPath.y + 0.001 > clipYpaths[0].StartPath.y && path.EndPath.y - 0.001 < clipYpaths[0].EndPath.y)
                            {
                                path.IsDeleted = true;
                            }
                        }
                    }
                    #endregion
                }
            }
            
            foreach (var item in paths)
            {
                float counter = increaseCounter();
                chunkDictionairy.Add(counter, item);
            }
            base.EventOccurred(data, type);

        }

    }

}
