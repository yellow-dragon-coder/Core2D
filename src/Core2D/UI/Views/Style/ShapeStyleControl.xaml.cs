﻿using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Core2D.UI.Views.Style
{
    /// <summary>
    /// Interaction logic for <see cref="ShapeStyleControl"/> xaml.
    /// </summary>
    public class ShapeStyleControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShapeStyleControl"/> class.
        /// </summary>
        public ShapeStyleControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initialize the Xaml components.
        /// </summary>
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
