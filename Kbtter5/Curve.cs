using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kbtter5
{
    public interface ICurve
    {
        IReadOnlyList<Point> Points { get; }
    }

    public class BezierCurve : ICurve
    {
        private List<Point> pnts;
        public IReadOnlyList<Point> Points { get { return pnts; } }
        public int Split { get; private set; }
        public IReadOnlyList<Point> ControlPoints { get; private set; }

        public BezierCurve(int split, params Point[] controls)
        {
            pnts = new List<Point>();
            Split = split;
            ControlPoints = controls;
            for (int i = 0; i < split; i++)
            {
                BuildPoints(ControlPoints, i * 1.0 / split);
            }
        }

        private void BuildPoints(IReadOnlyList<Point> points, double ratio)
        {
            if (points.Count == 1)
            {
                pnts.Add(points[0]);
                return;
            }
            else
            {
                var next = new Point[points.Count - 1];
                for (int i = 0; i < next.Length; i++)
                {
                    next[i].X = points[i].X + (points[i + 1].X - points[i].X) * ratio;
                    next[i].Y = points[i].Y + (points[i + 1].Y - points[i].Y) * ratio;
                }
                BuildPoints(next, ratio);
            }
        }
    }

    public class TargettingDummyCurve : ICurve
    {
        private List<Point> pnts;
        public IReadOnlyList<Point> Points { get { return pnts; } }

        public TargettingDummyCurve(Point start, Point target, double speed, int count)
        {
            pnts = new List<Point>();
            var ang = Math.Atan2(target.Y - start.Y, target.X - start.X);
            for (int i = 0; i < count; i++)
            {
                pnts.Add(new Point { X = start.X + Math.Cos(ang) * speed * i, Y = start.Y + Math.Sin(ang) * speed * i });
            }
        }
    }

}
