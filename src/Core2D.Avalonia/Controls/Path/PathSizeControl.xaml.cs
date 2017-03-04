﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Core2D.Avalonia.Controls.Path
{
    /// <summary>
    /// Interaction logic for <see cref="PathSizeControl"/> xaml.
    /// </summary>
    public class PathSizeControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PathSizeControl"/> class.
        /// </summary>
        public PathSizeControl()
        {
            this.InitializeComponent();
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
