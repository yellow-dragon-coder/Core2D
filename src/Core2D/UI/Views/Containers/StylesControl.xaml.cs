﻿using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Core2D.UI.Views.Containers
{
    /// <summary>
    /// Interaction logic for <see cref="StylesControl"/> xaml.
    /// </summary>
    public class StylesControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StylesControl"/> class.
        /// </summary>
        public StylesControl()
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
