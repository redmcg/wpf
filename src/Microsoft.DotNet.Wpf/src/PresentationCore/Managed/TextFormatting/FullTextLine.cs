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
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using MS.Internal;
using MS.Internal.Shaping;
using Common.TextFormatting;
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
            private TextMetrics                         _metrics;                       // Text metrics
            private StatusFlags                         _statusFlags;                   // status flags of the line
            private FullTextState                       _fullText;                      // full text state kept for collapsing purpose (only have it when StatusFlags.HasOverflowed is set)
            private TextDecorationCollection            _paragraphTextDecorations;      // Paragraph-level text decorations (or null if none)
            private Brush                               _defaultTextDecorationsBrush;   // Default brush for paragraph text decorations
            private TextFormattingMode                  _textFormattingMode;            // The TextFormattingMode of the line (Ideal or Display).
			private List<TextSpan<TextRun>>				_textRunSpans;					// List of TextRun objects in this line and their lengths

            [Flags]
            private enum StatusFlags
            {
                None                = 0,
                IsDisposed          = 0x00000001,
                HasOverflowed       = 0x00000002,
                BoundingBoxComputed = 0x00000004,
                RightToLeft         = 0x00000008,
                HasCollapsed        = 0x00000010,
                KeepState           = 0x00000020,
                IsTruncated         = 0x00000040,
                IsJustified         = 0x00000080, // Indicates whether the text alignment is set to justified
                                                  // This flag is needed later to decide how the metrics
                                                  // will be rounded in display mode when converted
                                                  // from ideal to real values.
            }

            internal FullTextLine(
                FormatSettings          settings,
                int                     cpFirst,
                int                     lineLength,
                int                     paragraphWidth,
                LineFlags               lineFlags
                )
                : this(settings.TextFormattingMode, settings.Pap.Justify, settings.TextSource.PixelsPerDip)
			{
                if (    (lineFlags & LineFlags.KeepState) != 0
                    ||  settings.Pap.AlwaysCollapsible)
                {
                    _statusFlags |= StatusFlags.KeepState;
                }

                int finiteFormatWidth = settings.GetFiniteFormatWidth(paragraphWidth);

                FullTextState fullText = FullTextState.Create(settings, cpFirst, finiteFormatWidth);

                // formatting the line
                FormatLine(
                    fullText,
                    cpFirst,
                    lineLength,
                    fullText.FormatWidth,
                    finiteFormatWidth,
                    paragraphWidth,
                    lineFlags,
                    null    // collapsingSymbol
                    );
			}

            /// <summary>
            /// Empty private constructor
            /// </summary>
            private FullTextLine(TextFormattingMode textFormattingMode, bool justify, double pixelsPerDip) : base(pixelsPerDip)
            {
                _textFormattingMode = textFormattingMode;
                if (justify)
                {
                    _statusFlags |= StatusFlags.IsJustified;
                }
                _metrics = new TextMetrics();
                _metrics._pixelsPerDip = pixelsPerDip;
				_textRunSpans = new List<TextSpan<TextRun>> ();
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

            /// <summary>
            /// format text line using LS
            /// </summary>
            /// <param name="fullText">state of the full text backing store</param>
            /// <param name="cpFirst">first cp to format</param>
            /// <param name="lineLength">character length of the line</param>
            /// <param name="formatWidth">width used to format</param>
            /// <param name="finiteFormatWidth">width used to detect overflowing of format result</param>
            /// <param name="paragraphWidth">paragraph width</param>
            /// <param name="lineFlags">line formatting control flags</param>
            /// <param name="collapsingSymbol">line end collapsing symbol</param>
            private void FormatLine(
                FullTextState           fullText,
                int                     cpFirst,
                int                     lineLength,
                int                     formatWidth,
                int                     finiteFormatWidth,
                int                     paragraphWidth,
                LineFlags               lineFlags,
                FormattedTextSymbols    collapsingSymbol
                )
            {
                _metrics._formatter = fullText.Formatter;
                Debug.Assert(_metrics._formatter != null);

                TextStore store = fullText.TextStore;
                TextStore markerStore = fullText.TextMarkerStore;
                FormatSettings settings = store.Settings;
                ParaProp pap = settings.Pap;
				var pixelsPerDip = settings.TextSource.PixelsPerDip;

                _paragraphTextDecorations = pap.TextDecorations;
                if (_paragraphTextDecorations != null)
                {
                    if (_paragraphTextDecorations.Count != 0)
                    {
                        _defaultTextDecorationsBrush = pap.DefaultTextDecorationsBrush;
                    }
                    else
                    {
                        _paragraphTextDecorations = null;
                    }
                }

				int pos = cpFirst;
				int content_ascent = 0;
				int content_descent = 0;

				while (lineLength <= 0 || cpFirst + lineLength > pos)
				{
					TextRun textRun;
					int runLength;
					CharacterBufferRange chars = settings.FetchTextRun(pos, cpFirst, out textRun, out runLength);

					if (lineLength > 0 && pos + runLength > cpFirst + lineLength)
					{
						runLength = cpFirst + lineLength - pos;
					}

					if (textRun is TextEndOfParagraph || textRun is TextEndOfLine)
					{
						pos += runLength;
						_textRunSpans.Add(new TextSpan<TextRun>(runLength, textRun));
						_metrics._cchNewline = runLength;
						break;
					}

					TextMetrics runMetrics = GetRunMetrics(fullText, textRun, cpFirst, pos, runLength);

					if (content_ascent < runMetrics._textAscent)
						content_ascent = runMetrics._textAscent;

					if (content_descent < runMetrics._textHeight - runMetrics._textAscent)
						content_descent = runMetrics._textHeight - runMetrics._textAscent;

					_metrics._textWidth += runMetrics._textWidth;

					pos += runLength;
					_textRunSpans.Add(new TextSpan<TextRun>(runLength, textRun));
				}

				_metrics._pixelsPerDip = pixelsPerDip;
				_metrics._cchLength = pos - cpFirst;
				_metrics._textWidthAtTrailing = _metrics._textWidth; // FIXME

				if (pap.LineHeight > 0)
				{
                    // Host specifies line height, honor it.
                    _metrics._height = pap.LineHeight;
                    _metrics._baselineOffset = (int)Math.Round(
                        _metrics._height
                        * pap.DefaultTypeface.Baseline(pap.EmSize, Constants.DefaultIdealToReal, pixelsPerDip, _textFormattingMode)
                        / pap.DefaultTypeface.LineSpacing(pap.EmSize, Constants.DefaultIdealToReal, pixelsPerDip, _textFormattingMode)
                        );
				}

				if (content_ascent > 0)
				{
					_metrics._textAscent = content_ascent;
					_metrics._textHeight = content_ascent + content_descent;

					// TODO: VerticalAdjust
				}
				else
				{
                    // Line is empty so text height and text baseline are based on the default typeface;
                    // it doesn't make sense even for an emtpy line to have zero text height
                    _metrics._textAscent = (int)Math.Round(pap.DefaultTypeface.Baseline(pap.EmSize, Constants.DefaultIdealToReal, pixelsPerDip, _textFormattingMode));
                    _metrics._textHeight = (int)Math.Round(pap.DefaultTypeface.LineSpacing(pap.EmSize, Constants.DefaultIdealToReal, pixelsPerDip, _textFormattingMode));
				}

				if (_metrics._height <= 0)
				{
                    _metrics._height = _metrics._textHeight;
                    _metrics._baselineOffset = _metrics._textAscent;
				}

                if (collapsingSymbol == null)
                {
                    // overflow detection for potential collapsible line
                    if (_metrics._textStart + _metrics._textWidthAtTrailing > finiteFormatWidth)
                    {
                        bool hasOverflowed = true;
                        if (_textFormattingMode == TextFormattingMode.Display)
                        {
                            // apply display-mode rounding before checking for overflow
                            double realWidth = Width;
                            double realFormatWidth = _metrics._formatter.IdealToReal(finiteFormatWidth, PixelsPerDip);
                            hasOverflowed = (TextFormatterImp.CompareReal(realWidth, realFormatWidth, PixelsPerDip, _textFormattingMode) > 0);
                        }

                        if (hasOverflowed)
                        {
                            // line has overflowed
                            _statusFlags |= StatusFlags.HasOverflowed;

                            // let's keep the full text state around. We'll need it later for collapsing
                            _fullText = fullText;
                        }
                    }
                }
			}

			private int GetShapeableSymbolsWidth(TextShapeableSymbols textRun)
			{
				int result = 0;
				int text_length = textRun.Length;
				int[] advance_widths = new int[text_length];
				GCHandle pin_handle;
				CharacterBuffer char_buf = textRun.CharacterBufferReference.CharacterBuffer;
				unsafe
				{
					IntPtr run_characters = char_buf.PinAndGetCharacterPointer(textRun.CharacterBufferReference.OffsetToFirstChar, out pin_handle);
					try
					{
						fixed (int* widths_ptr = advance_widths)
						{
							textRun.GetAdvanceWidthsUnshaped((char*)run_characters.ToPointer(), text_length, TextFormatterImp.ToIdeal, widths_ptr);
						}
					}
					finally
					{
						char_buf.UnpinCharacterPointer(pin_handle);
					}
				}

				for (int i=0; i<text_length; i++)
				{
					result += advance_widths[i];
				}

				return result;
			}

			private TextMetrics GetRunMetrics(FullTextState textState, TextRun textRun, int lineStart, int runStart, int runLength)
			{
				if (textRun is TextCharacters)
				{
					var textChars = (TextCharacters)textRun;
					var result = new TextMetrics();
					var props = textChars.Properties;
					var typeface = props.Typeface;
					var ideal_emsize = TextFormatterImp.RealToIdeal(props.FontRenderingEmSize);

                    result._textAscent = (int)Math.Round(typeface.Baseline(ideal_emsize, Constants.DefaultIdealToReal, props.PixelsPerDip, _textFormattingMode));
                    result._textHeight = (int)Math.Round(typeface.LineSpacing(ideal_emsize, Constants.DefaultIdealToReal, props.PixelsPerDip, _textFormattingMode));
					result._height = result._textHeight;
					result._baselineOffset = result._textAscent;

					// width calculation
					CultureInfo digitCulture = null;

					if (!textChars.Properties.Typeface.Symbol)
						digitCulture = textState.TextStore.Settings.DigitState.DigitCulture;

					// TODO: calculate bidi stuff using TextStore and pass that in
					var shapeable_symbols_list = ((ITextSymbols)(textChars)).GetTextShapeableSymbols(
						textState.Formatter.GlyphingCache,
						textChars.CharacterBufferReference,
						runLength,
						false, // rightToLeft
						textState.TextStore.Settings.Pap.RightToLeft, // isRightToLeftParagraph
						digitCulture,
						null, // TextModifierScope
						_textFormattingMode,
						false // isSideways
						);
					// for each shapeablesymbols:
					foreach (var shapeable_symbols in shapeable_symbols_list)
					{
						result._textWidth += GetShapeableSymbolsWidth(shapeable_symbols);
					}

					return result;
				}
				else
				{
					throw new NotImplementedException(String.Format("Managed.TextFormatting.FullTextLine.GetRunMetrics for {0}", textRun.GetType().FullName));
				}
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
                if ((_statusFlags & StatusFlags.IsDisposed) != 0)
                {
                    throw new ObjectDisposedException(SR.Get(SRID.TextLineHasBeenDisposed));
                }

				return new List<TextSpan<TextRun>> (_textRunSpans);
			}

			public override IEnumerable<IndexedGlyphRun> GetIndexedGlyphRuns()
			{
				throw new NotImplementedException("Managed.TextFormatting.FullTextLine.GetIndexedGlyphRuns");
			}

			public override TextLineBreak GetTextLineBreak()
			{
				// FIXME: Under what circumstances does this return non-null?
				// Apparently, specifying a too small width isn't enough.
				return null;
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
					return _metrics.Length;
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
					return _metrics.NewlineLength;
				}
			}

			public override double Start
			{
				get
				{
					return _metrics.Start;
				}
			}

			public override double Width
			{
				get
				{
					return _metrics.Width;
				}
			}

			public override double WidthIncludingTrailingWhitespace
			{
				get
				{
					return _metrics.WidthIncludingTrailingWhitespace;
				}
			}

			public override double Height
			{
				get
				{
					return _metrics.Height;
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
					return _metrics.Baseline;
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
                get { return (_statusFlags & StatusFlags.HasOverflowed) != 0; }
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

