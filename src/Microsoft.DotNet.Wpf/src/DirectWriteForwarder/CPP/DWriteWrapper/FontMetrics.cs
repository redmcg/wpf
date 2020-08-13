// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices;

namespace MS.Internal.Text.TextInterface
{
    /// <summary>
    /// Specifies the metrics of a font face that
    /// are applicable to all glyphs within the font face.
    ///
    /// COMPATIBILITY WARNING: The layout of this struct must match exactly the layout of DWRITE_FONT_METRICS from DWrite.h
    /// We will perform an unsafe cast from this type to DWRITE_FONT_METRICS when obtaining glyph metrics. We do this for speed
    /// because we have to be able to obtain GlyphMetrics quickly, and the heap allocations and copy time were causing severe
    /// performance degradations.
    ///
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]    
    public struct FontMetrics
    {
        /// <summary>
        /// The number of font design units per em unit.
        /// Font files use their own coordinate system of font design units.
        /// A font design unit is the smallest measurable unit in the em square,
        /// an imaginary square that is used to size and align glyphs.
        /// The concept of em square is used as a reference scale factor when defining font size and device transformation semantics.
        /// The size of one em square is also commonly used to compute the paragraph identation value.
        /// </summary>
        [FieldOffset(0)] 
        public ushort DesignUnitsPerEm;

        /// <summary>
        /// Ascent value of the font face in font design units.
        /// Ascent is the distance from the top of font character alignment box to English baseline.
        /// </summary>
        [FieldOffset(2)] 
        public ushort Ascent;

        /// <summary>
        /// Descent value of the font face in font design units.
        /// Descent is the distance from the bottom of font character alignment box to English baseline.
        /// </summary>
        [FieldOffset(4)] 
        public ushort Descent;

        /// <summary>
        /// Line gap in font design units.
        /// Recommended additional white space to add between lines to improve legibility. The recommended line spacing 
        /// (baseline-to-baseline distance) is thus the sum of ascent, descent, and lineGap. The line gap is usually 
        /// positive or zero but can be negative, in which case the recommended line spacing is less than the height
        /// of the character alignment box.
        /// </summary>
        [FieldOffset(6)] 
        public short LineGap;

        /// <summary>
        /// Cap height value of the font face in font design units.
        /// Cap height is the distance from English baseline to the top of a typical English capital.
        /// Capital "H" is often used as a reference character for the purpose of calculating the cap height value.
        /// </summary>
        [FieldOffset(8)] 
        public ushort CapHeight;

        /// <summary>
        /// x-height value of the font face in font design units.
        /// x-height is the distance from English baseline to the top of lowercase letter "x", or a similar lowercase character.
        /// </summary>
        [FieldOffset(10)]         
        public ushort XHeight;

        /// <summary>
        /// The underline position value of the font face in font design units.
        /// Underline position is the position of underline relative to the English baseline.
        /// The value is usually made negative in order to place the underline below the baseline.
        /// </summary>
        [FieldOffset(12)]         
        public short UnderlinePosition;

        /// <summary>
        /// The suggested underline thickness value of the font face in font design units.
        /// </summary>
        [FieldOffset(14)]
        public ushort UnderlineThickness;

        /// <summary>
        /// The strikethrough position value of the font face in font design units.
        /// Strikethrough position is the position of strikethrough relative to the English baseline.
        /// The value is usually made positive in order to place the strikethrough above the baseline.
        /// </summary>
        [FieldOffset(16)] 
        public short StrikethroughPosition;

        /// <summary>
        /// The suggested strikethrough thickness value of the font face in font design units.
        /// </summary>
        [FieldOffset(18)] 
        public ushort StrikethroughThickness;

        /// <summary>
        /// The baseline of the font face.
        /// </summary>
        public double Baseline
        {
            get
            {
                return (double)(this.Ascent + this.LineGap * 0.5)  / DesignUnitsPerEm;
            }
        }

        /// <summary>
        /// The linespacing of the font face.
        /// </summary>
        public double LineSpacing
        {
            get
            {
                return (double)(this.Ascent + this.Descent + this.LineGap) / DesignUnitsPerEm;
            }
        }
    }
}
