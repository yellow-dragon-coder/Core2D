﻿using System;
using System.Collections.Generic;
using Core2D.History;
using Core2D.Shapes;
using System.Linq;

namespace Core2D.Editor.Layout
{
    public static class BoxLayout
    {
        private static void MoveShapeByWithHistory(IBaseShape shape, double dx, double dy, IHistory history)
        {
            var previous = new { DeltaX = -dx, DeltaY = -dy, shape };
            var next = new { DeltaX = dx, DeltaY = dy, shape };
            history.Snapshot(previous, next, (s) => s.shape.Move(null, s.DeltaX, s.DeltaY));
            shape.Move(null, dx, dy);
        }

        public static void Stack(IEnumerable<IBaseShape> shapes, StackMode mode, IHistory history)
        {
            var boxes = new List<ShapeBox>();

            foreach (var shape in shapes)
            {
                boxes.Add(new ShapeBox(shape));
            }

            if (boxes.Count < 2)
            {
                return;
            }

            var bounds = new GroupBox(boxes);

            switch (mode)
            {
                case StackMode.Horizontal:
                    {
                        boxes.Sort(ShapeBox.CompareLeft);
                        double offset = boxes[0].Left + boxes[0].Width;
                        for (int i = 1; i <= boxes.Count - 1; i++)
                        {
                            var box = boxes[i];
                            double dx = offset - box.Left;
                            double dy = 0.0;
                            MoveShapeByWithHistory(box.Shape, dx, dy, history);
                            offset += box.Width;
                        }
                    }
                    break;
                case StackMode.Vertical:
                    {
                        boxes.Sort(ShapeBox.CompareTop);
                        double offset = boxes[0].Top + boxes[0].Height;
                        for (int i = 1; i <= boxes.Count - 1; i++)
                        {
                            var box = boxes[i];
                            double dx = 0.0;
                            double dy = offset - box.Top;
                            MoveShapeByWithHistory(box.Shape, dx, dy, history);
                            offset += box.Height;
                        }
                    }
                    break;
            }
        }

        public static void Distribute(IEnumerable<IBaseShape> shapes, DistributeMode mode, IHistory history)
        {
            var boxes = new List<ShapeBox>();

            foreach (var shape in shapes)
            {
                boxes.Add(new ShapeBox(shape));
            }

            if (boxes.Count <= 2)
            {
                return;
            }

            var bounds = new GroupBox(boxes);

            double sw = 0.0;
            double sh = 0.0;

            foreach (var box in boxes)
            {
                sw += box.Width;
                sh += box.Height;
            }

            double gaph = (bounds.Width - sw) / (boxes.Count - 1);
            double gapv = (bounds.Height - sh) / (boxes.Count - 1);

            switch (mode)
            {
                case DistributeMode.Horizontal:
                    {
                        boxes.Sort(ShapeBox.CompareLeft);
                        double offset = boxes[0].Left + boxes[0].Width + gaph;
                        for (int i = 1; i <= boxes.Count - 2; i++)
                        {
                            var box = boxes[i];
                            double dx = offset - box.Left;
                            double dy = 0.0;
                            MoveShapeByWithHistory(box.Shape, dx, dy, history);
                            offset += box.Width + gaph;
                        }
                    }
                    break;
                case DistributeMode.Vertical:
                    {
                        boxes.Sort(ShapeBox.CompareTop);
                        double offset = boxes[0].Top + boxes[0].Height + gapv;
                        for (int i = 1; i <= boxes.Count - 2; i++)
                        {
                            var box = boxes[i];
                            double dx = 0.0;
                            double dy = offset - box.Top;
                            MoveShapeByWithHistory(box.Shape, dx, dy, history);
                            offset += box.Height + gapv;
                        }
                    }
                    break;
            }
        }

        public static void Align(IEnumerable<IBaseShape> shapes, AlignMode mode, IHistory history)
        {
            var boxes = new List<ShapeBox>();

            foreach (var shape in shapes)
            {
                boxes.Add(new ShapeBox(shape));
            }

            if (boxes.Count <= 1)
            {
                return;
            }

            var bounds = new GroupBox(boxes);

            foreach (var box in boxes)
            {
                double dx = 0.0;
                double dy = 0.0;

                switch (mode)
                {
                    case AlignMode.Left:
                        dx = bounds.Left - box.Left;
                        break;
                    case AlignMode.Centered:
                        dx = bounds.CenterX - ((box.Left + box.Right) / 2.0);
                        break;
                    case AlignMode.Right:
                        dx = bounds.Right - box.Right;
                        break;
                    case AlignMode.Top:
                        dy = bounds.Top - box.Top;
                        break;
                    case AlignMode.Center:
                        dy = bounds.CenterY - ((box.Top + box.Bottom) / 2.0);
                        break;
                    case AlignMode.Bottom:
                        dy = bounds.Bottom - box.Bottom;
                        break;
                }

                if (dx != 0.0 || dy != 0.0)
                {
                    MoveShapeByWithHistory(box.Shape, dx, dy, history);
                }
            }
        }

        public static void Flip(IEnumerable<IBaseShape> shapes, FlipMode mode, IHistory history)
        {
            var boxes = new List<ShapeBox>();

            foreach (var shape in shapes)
            {
                boxes.Add(new ShapeBox(shape));
            }

            if (boxes.Count <= 0)
            {
                return;
            }

            var bounds = new GroupBox(boxes);

            switch (mode)
            {
                case FlipMode.Horizontal:
                    {
                        var previous = new List<(IPointShape point, double x)>();
                        var next = new List<(IPointShape point, double x)>();

                        foreach (var point in boxes.SelectMany(box => box.Points).Distinct())
                        {
                            double x = bounds.Left + (bounds.Width + bounds.Left) - point.X;
                            previous.Add((point, point.X));
                            next.Add((point, x));
                            point.X = x;
                        }

                        history.Snapshot(previous, next, (p) => previous.ForEach(p => p.point.X = p.x));
                    }
                    break;
                case FlipMode.Vertical:
                    {
                        var previous = new List<(IPointShape point, double y)>();
                        var next = new List<(IPointShape point, double y)>();

                        foreach (var point in boxes.SelectMany(box => box.Points).Distinct())
                        {
                            double y = bounds.Top + (bounds.Height + bounds.Top) - point.Y;
                            previous.Add((point, point.Y));
                            next.Add((point, y));
                            point.Y = y;
                        }

                        history.Snapshot(previous, next, (p) => previous.ForEach(p => p.point.Y = p.y));
                    }
                    break;
            }
        }

        public static void Rotate(IEnumerable<IBaseShape> shapes, double angle, IHistory history)
        {
            var boxes = new List<ShapeBox>();

            foreach (var shape in shapes)
            {
                boxes.Add(new ShapeBox(shape));
            }

            if (boxes.Count <= 0)
            {
                return;
            }

            var bounds = new GroupBox(boxes);

            var previous = new List<(IPointShape point, double x, double y)>();
            var next = new List<(IPointShape point, double x, double y)>();

            var radians = angle * Math.PI / 180.0;
            var centerX = bounds.CenterX;
            var centerY = bounds.CenterY;

            foreach (var point in boxes.SelectMany(box => box.Points).Distinct())
            {
                Rotate(point, radians, centerX, centerY, out var x, out var y);
                previous.Add((point, point.X, point.Y));
                next.Add((point, x, y));
                point.X = x;
                point.Y = y;
            }

            history.Snapshot(previous, next, (p) => previous.ForEach(p =>
            {
                p.point.X = p.x;
                p.point.Y = p.y;
            }));

            static void Rotate(IPointShape point, double radians, double centerX, double centerY, out double x, out double y)
            {
                x = (point.X - centerX) * Math.Cos(radians) - (point.Y - centerY) * Math.Sin(radians) + centerX;
                y = (point.X - centerX) * Math.Sin(radians) + (point.Y - centerY) * Math.Cos(radians) + centerY;
            }
        }
    }
}
