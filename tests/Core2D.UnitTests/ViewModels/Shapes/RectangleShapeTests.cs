﻿using Core2D.Interfaces;
using Xunit;

namespace Core2D.Shapes.UnitTests
{
    public class RectangleShapeTests
    {
        private readonly IFactory _factory = new Factory();

        [Fact]
        [Trait("Core2D.Shapes", "Shapes")]
        public void Inherits_From_TextShape()
        {
            var style = _factory.CreateShapeStyle();
            var target = _factory.CreateRectangleShape(0, 0, 0, 0, style);
            Assert.True(target is TextShape);
        }
    }
}
