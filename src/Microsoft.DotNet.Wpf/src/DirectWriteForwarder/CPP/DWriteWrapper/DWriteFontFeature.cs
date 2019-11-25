// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices;

namespace MS.Internal.Text.TextInterface
{
    
    /// <summary>
    /// Specifies properties used to identify and execute typographic feature in the font.
    ///
    /// COMPATIBILITY WARNING: The layout of this struct must match exactly the layout of DWRITE_FONT_FEATURE from DWrite.h
    /// We will perform an unsafe cast from this type to DWRITE_FONT_FEATURE when obtaining Font Features. We do this to improve perf.
    ///
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct DWriteFontFeature
    {        
        /// <summary>
        /// The feature OpenType name identifier.
        /// </summary>
        DWriteFontFeatureTag nameTag;

        /// <summary>
        /// Execution parameter of the feature.
        /// </summary>
        /// <remarks>
        /// The parameter should be non-zero to enable the feature.  Once enabled, a feature can't be disabled again within
        /// the same range.  Features requiring a selector use this value to indicate the selector index. 
        /// </remarks>
        uint parameter;

        DWriteFontFeature(DWriteFontFeatureTag dwriteNameTag, uint dwriteParameter)
        {
            nameTag = dwriteNameTag;
            parameter = dwriteParameter;
        }
    }
}
