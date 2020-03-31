﻿using System;
using System.Collections.Generic;
using System.Linq;
using Core2D.Editor;
using Core2D.Interfaces;
using Core2D.Shapes;
using Core2D.Style;
using SkiaSharp;

namespace Core2D.Renderer.SkiaSharp
{
    public class SkiaSharpPathConverter : IPathConverter
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="SkiaSharpPathConverter"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public SkiaSharpPathConverter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private SKPathOp ToSKPathOp(PathOp op)
        {
            return op switch
            {
                PathOp.Intersect => SKPathOp.Intersect,
                PathOp.Union => SKPathOp.Union,
                PathOp.Xor => SKPathOp.Xor,
                PathOp.ReverseDifference => SKPathOp.ReverseDifference,
                _ => SKPathOp.Difference,
            };
        }

        private void Op(SKPathOp op, IList<SKPath> paths, out SKPath result, out bool haveResult)
        {
            haveResult = false;
            result = new SKPath(paths[0]) { FillType = paths[0].FillType };

            if (paths.Count == 1)
            {
                using var empty = new SKPath() { FillType = paths[0].FillType };
                result = empty.Op(paths[0], op);
                haveResult = true;
            }
            else
            {
                for (int i = 1; i < paths.Count; i++)
                {
                    var next = result.Op(paths[i], op);
                    if (next != null)
                    {
                        result.Dispose();
                        result = next;
                        haveResult = true;
                    }
                }
            }
        }

        /// <inheritdoc/>
        public IPathShape ToPathShape(IEnumerable<IBaseShape> shapes)
        {
            var editor = _serviceProvider.GetService<IProjectEditor>();
            var factory = _serviceProvider.GetService<IFactory>();
            var first = shapes.FirstOrDefault();
            var style = (IShapeStyle)first.Style?.Copy(null);
            var path = PathGeometryConverter.ToSKPath(shapes, 0.0, 0.0, (value) => (float)value);
            var geometry = PathGeometryConverter.ToPathGeometry(path, 0.0, 0.0, factory, editor.Project.Options.PointShape);
            var pathShape = factory.CreatePathShape(
                "Path",
                style,
                geometry,
                first.IsStroked,
                first.IsFilled);
            return pathShape;
        }

        /// <inheritdoc/>
        public IPathShape ToPathShape(IBaseShape shape)
        {
            var editor = _serviceProvider.GetService<IProjectEditor>();
            var factory = _serviceProvider.GetService<IFactory>();
            var style = (IShapeStyle)shape.Style?.Copy(null);
            var path = PathGeometryConverter.ToSKPath(shape, 0.0, 0.0, (value) => (float)value);
            var geometry = PathGeometryConverter.ToPathGeometry(path, 0.0, 0.0, factory, editor.Project.Options.PointShape);
            var pathShape = factory.CreatePathShape(
                "Path",
                style,
                geometry,
                shape.IsStroked,
                shape.IsFilled);
            return pathShape;
        }

        /// <inheritdoc/>
        public IPathShape ToStrokePathShape(IBaseShape shape)
        {
            var path = PathGeometryConverter.ToSKPath(shape, 0.0, 0.0, (value) => (float)value);
            if (path == null)
            {
                return null;
            }
            var editor = _serviceProvider.GetService<IProjectEditor>();
            var factory = _serviceProvider.GetService<IFactory>();
            var style = (IShapeStyle)shape.Style?.Copy(null);
            using var paint = SkiaSharpRenderer.ToSKPaintPen(shape.Style, (value) => (float)value, 96.0, 96.0, true);
            var result = paint.GetFillPath(path, 1.0f);
            if (result != null)
            {
                Op(SKPathOp.Union, new[] { result, result }, out var union, out bool haveResult);
                if (haveResult == false || union == null || union.IsEmpty)
                {
                    result.Dispose();
                    return null;
                }
                var geometry = PathGeometryConverter.ToPathGeometry(union, 0.0, 0.0, factory, editor.Project.Options.PointShape);
                var pathShape = factory.CreatePathShape(
                    "Path",
                    style,
                    geometry,
                    shape.IsStroked,
                    shape.IsFilled);
                result.Dispose();
                union.Dispose();
                return pathShape;
            }
            return null;
        }

        /// <inheritdoc/>
        public IPathShape ToFillPathShape(IBaseShape shape)
        {
            var path = PathGeometryConverter.ToSKPath(shape, 0.0, 0.0, (value) => (float)value);
            if (path == null)
            {
                return null;
            }
            var editor = _serviceProvider.GetService<IProjectEditor>();
            var factory = _serviceProvider.GetService<IFactory>();
            var style = (IShapeStyle)shape.Style?.Copy(null);
            using var paint = SkiaSharpRenderer.ToSKPaintBrush(shape.Style.Fill, true);
            var result = paint.GetFillPath(path, 1.0f);
            if (result != null)
            {
                Op(SKPathOp.Union, new[] { result, result }, out var union, out bool haveResult);
                if (haveResult == false || union == null || union.IsEmpty)
                {
                    result.Dispose();
                    return null;
                }
                var geometry = PathGeometryConverter.ToPathGeometry(union, 0.0, 0.0, factory, editor.Project.Options.PointShape);
                var pathShape = factory.CreatePathShape(
                    "Path",
                    style,
                    geometry,
                    shape.IsStroked,
                    shape.IsFilled);
                result.Dispose();
                union.Dispose();
                return pathShape;
            }
            return null;
        }

        /// <inheritdoc/>
        public IPathShape Op(IEnumerable<IBaseShape> shapes, PathOp op)
        {
            if (shapes == null || shapes.Count() <= 0)
            {
                return null;
            }

            var paths = new List<SKPath>();

            foreach (var s in shapes)
            {
                var path = PathGeometryConverter.ToSKPath(s, 0.0, 0.0, (value) => (float)value);
                if (path != null)
                {
                    paths.Add(path);
                }
            }

            if (paths == null || paths.Count <= 0)
            {
                return null;
            }

            Op(ToSKPathOp(op), paths, out var result, out var haveResult);
            if (haveResult == false || result == null || result.IsEmpty)
            {
                return null;
            }

            var editor = _serviceProvider.GetService<IProjectEditor>();
            var factory = _serviceProvider.GetService<IFactory>();
            var shape = shapes.FirstOrDefault();
            var style = (IShapeStyle)shape.Style?.Copy(null);
            var geometry = PathGeometryConverter.ToPathGeometry(result, 0.0, 0.0, factory, editor.Project.Options.PointShape);
            var pathShape = factory.CreatePathShape(
                "Path",
                style,
                geometry,
                shape.IsStroked,
                shape.IsFilled);
            result.Dispose();
            return pathShape;
        }
    }
}
