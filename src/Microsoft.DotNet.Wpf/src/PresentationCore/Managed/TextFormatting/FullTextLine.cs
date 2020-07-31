// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//
//
//  Contents:  Complex implementation of TextLine
//
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Security;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using MS.Internal;
using MS.Internal.Shaping;
using ITextMetrics = MS.Internal.TextFormatting.ITextMetrics;

using SR = MS.Internal.PresentationCore.SR;
using SRID = MS.Internal.PresentationCore.SRID;


namespace Managed.TextFormatting
{
    /// <remarks>
    /// Make FullTextLine nested type of TextMetrics to allow full access to TextMetrics private members
    /// </remarks>
    internal partial struct TextMetrics : ITextMetrics
    {
        /// <summary>
        /// Complex implementation of TextLine
        ///
        /// TextLine implementation around
        ///     o   Line Services
        ///     o   OpenType Services Library
        ///     o   Implementation of Unicode Bidirectional algorithm
        ///     o   Complex script itemizer
        ///     o   Composite font with generic glyph hunting algorithm
        /// </summary>
        internal class FullTextLine : TextLine
        {
            internal FullTextLine(
                FormatSettings          settings,
                int                     cpFirst,
                int                     lineLength,
                int                     paragraphWidth,
                LineFlags               lineFlags
                )
			{
			}

			~FullTextLine()
			{
				DisposeInternal(true);
			}

			public override void Dispose()
			{
				DisposeInternal(false);
				GC.SuppressFinalize(this);
			}

			private void DisposeInternal(bool finalizing)
			{
			}

			public override void Draw(DrawingContext drawingContext, Point origin, InvertAxes inversion)
			{
			}

			public override TextLine Collapse(params TextCollapsingProperties[] collapsingPropertiesList)
			{
				throw new NotImplementedException("Managed.TextFormatting.FullTextLine.Collapse");
			}

			public override IList<TextCollapsedRange> GetTextCollapsedRanges()
			{
				throw new NotImplementedException("Managed.TextFormatting.FullTextLine.GetTextCollapsedRanges");
			}

			public override CharacterHit GetCharacterHitFromDistance(double distance)
			{
				throw new NotImplementedException("Managed.TextFormatting.FullTextLine.GetCharacterHitFromDistance");
			}

			public override double GetDistanceFromCharacterHit(CharacterHit characterHit)
			{
				throw new NotImplementedException("Managed.TextFormatting.FullTextLine.GetDistanceFromCharacterHit");
			}

			public override CharacterHit GetNextCaretCharacterHit(CharacterHit characterHit)
			{
				throw new NotImplementedException("Managed.TextFormatting.FullTextLine.GetNextCaretCharacterHit");
			}

			public override CharacterHit GetPreviousCaretCharacterHit(CharacterHit characterHit)
			{
				throw new NotImplementedException("Managed.TextFormatting.FullTextLine.GetPreviousCaretCharacterHit");
			}

			public override CharacterHit GetBackspaceCaretCharacterHit(CharacterHit characterHit)
			{
				throw new NotImplementedException("Managed.TextFormatting.FullTextLine.GetBackspaceCaretCharacterHit");
			}

			public override IList<TextBounds> GetTextBounds(int firstTextSourceCharacterIndex, int textLength)
			{
				throw new NotImplementedException("Managed.TextFormatting.FullTextLine.GetTextBounds");
			}

			public override IList<TextSpan<TextRun>> GetTextRunSpans()
			{
				throw new NotImplementedException("Managed.TextFormatting.FullTextLine.GetTextRunSpans");
			}

			public override IEnumerable<IndexedGlyphRun> GetIndexedGlyphRuns()
			{
				throw new NotImplementedException("Managed.TextFormatting.FullTextLine.GetIndexedGlyphRuns");
			}

			public override TextLineBreak GetTextLineBreak()
			{
				throw new NotImplementedException("Managed.TextFormatting.FullTextLine.GetTextLineBreak");
			}

			public override int TrailingWhitespaceLength
			{
				get
				{
					throw new NotImplementedException("Managed.TextFormatting.FullTextLine.get_TrailingWhitespaceLength");
				}
			}

			public override int Length
			{
				get
				{
					throw new NotImplementedException("Managed.TextFormatting.FullTextLine.get_Length");
				}
			}

			public override int DependentLength
			{
				get
				{
					throw new NotImplementedException("Managed.TextFormatting.FullTextLine.get_DependentLength");
				}
			}

			public override int NewlineLength
			{
				get
				{
					throw new NotImplementedException("Managed.TextFormatting.FullTextLine.get_NewlineLength");
				}
			}

			public override double Start
			{
				get
				{
					throw new NotImplementedException("Managed.TextFormatting.FullTextLine.get_Start");
				}
			}

			public override double Width
			{
				get
				{
					throw new NotImplementedException("Managed.TextFormatting.FullTextLine.get_Width");
				}
			}

			public override double WidthIncludingTrailingWhitespace
			{
				get
				{
					throw new NotImplementedException("Managed.TextFormatting.FullTextLine.get_WidthIncludingTrailingWhitespace");
				}
			}

			public override double Height
			{
				get
				{
					throw new NotImplementedException("Managed.TextFormatting.FullTextLine.get_Height");
				}
			}

			public override double TextHeight
			{
				get
				{
					throw new NotImplementedException("Managed.TextFormatting.FullTextLine.get_TextHeight");
				}
			}

			public override double Baseline
			{
				get
				{
					throw new NotImplementedException("Managed.TextFormatting.FullTextLine.get_Baseline");
				}
			}

			public override double TextBaseline
			{
				get
				{
					throw new NotImplementedException("Managed.TextFormatting.FullTextLine.get_TextBaseline");
				}
			}

			public override double MarkerBaseline
			{
				get
				{
					throw new NotImplementedException("Managed.TextFormatting.FullTextLine.get_MarkerBaseline");
				}
			}

			public override double MarkerHeight
			{
				get
				{
					throw new NotImplementedException("Managed.TextFormatting.FullTextLine.get_MarkerHeight");
				}
			}

			public override double Extent
			{
				get
				{
					throw new NotImplementedException("Managed.TextFormatting.FullTextLine.get_Extent");
				}
			}

			public override double OverhangLeading
			{
				get
				{
					throw new NotImplementedException("Managed.TextFormatting.FullTextLine.get_OverhangLeading");
				}
			}

			public override double OverhangTrailing
			{
				get
				{
					throw new NotImplementedException("Managed.TextFormatting.FullTextLine.get_OverhangTrailing");
				}
			}

			public override double OverhangAfter
			{
				get
				{
					throw new NotImplementedException("Managed.TextFormatting.FullTextLine.get_OverhangAfter");
				}
			}

			public override bool HasOverflowed
			{
				get
				{
					throw new NotImplementedException("Managed.TextFormatting.FullTextLine.get_HasOverflowed");
				}
			}

			public override bool HasCollapsed
			{
				get
				{
					throw new NotImplementedException("Managed.TextFormatting.FullTextLine.get_HasCollapsed");
				}
			}

			public override bool IsTruncated
			{
				get
				{
					throw new NotImplementedException("Managed.TextFormatting.FullTextLine.get_IsTruncated");
				}
			}
        }
    }
}

