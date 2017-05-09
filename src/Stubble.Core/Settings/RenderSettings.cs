﻿// <copyright file="RenderSettings.cs" company="Stubble Authors">
// Copyright (c) Stubble Authors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Stubble.Core.Settings
{
    /// <summary>
    /// The settings to be used when Rendering a Mustache Template
    /// </summary>
    public class RenderSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether values should be recursively
        /// looked up in the render context. (faster without)
        /// </summary>
        public bool SkipRecursiveLookup { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether exceptions should be thrown if
        /// tags are defined which don't exist in the context
        /// </summary>
        public bool ThrowOnDataMiss { get; set; }

        /// <summary>
        /// Gets the default render settings
        /// </summary>
        /// <returns>the default <see cref="RenderSettings"/></returns>
        public static RenderSettings GetDefaultRenderSettings()
        {
            return new RenderSettings
            {
                SkipRecursiveLookup = false,
                ThrowOnDataMiss = false
            };
        }
    }
}