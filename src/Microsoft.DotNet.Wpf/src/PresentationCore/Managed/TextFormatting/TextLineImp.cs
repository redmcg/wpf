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

using SR = MS.Internal.PresentationCore.SR;
using SRID = MS.Internal.PresentationCore.SRID;

namespace Managed.TextFormatting
{
	internal class TextLineImp : TextLine
	{
		internal TextLineImp()
		{
		}

		~TextLineImp()
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
			throw new NotImplementedException("Managed.TextFormatting.TextLineImp.Collapse");
		}

		public override IList<TextCollapsedRange> GetTextCollapsedRanges()
		{
			throw new NotImplementedException("Managed.TextFormatting.TextLineImp.GetTextCollapsedRanges");
		}

		public override CharacterHit GetCharacterHitFromDistance(double distance)
		{
			throw new NotImplementedException("Managed.TextFormatting.TextLineImp.GetCharacterHitFromDistance");
		}

		public override double GetDistanceFromCharacterHit(CharacterHit characterHit)
		{
			throw new NotImplementedException("Managed.TextFormatting.TextLineImp.GetDistanceFromCharacterHit");
		}

		public override CharacterHit GetNextCaretCharacterHit(CharacterHit characterHit)
		{
			throw new NotImplementedException("Managed.TextFormatting.TextLineImp.GetNextCaretCharacterHit");
		}

		public override CharacterHit GetPreviousCaretCharacterHit(CharacterHit characterHit)
		{
			throw new NotImplementedException("Managed.TextFormatting.TextLineImp.GetPreviousCaretCharacterHit");
		}

		public override CharacterHit GetBackspaceCaretCharacterHit(CharacterHit characterHit)
		{
			throw new NotImplementedException("Managed.TextFormatting.TextLineImp.GetBackspaceCaretCharacterHit");
		}

		public override IList<TextBounds> GetTextBounds(int firstTextSourceCharacterIndex, int textLength)
		{
			throw new NotImplementedException("Managed.TextFormatting.TextLineImp.GetTextBounds");
		}

		public override IList<TextSpan<TextRun>> GetTextRunSpans()
		{
			throw new NotImplementedException("Managed.TextFormatting.TextLineImp.GetTextRunSpans");
		}

        public override IEnumerable<IndexedGlyphRun> GetIndexedGlyphRuns()
		{
			throw new NotImplementedException("Managed.TextFormatting.TextLineImp.GetIndexedGlyphRuns");
		}

		public override TextLineBreak GetTextLineBreak()
		{
			throw new NotImplementedException("Managed.TextFormatting.TextLineImp.GetTextLineBreak");
		}

		public override int TrailingWhitespaceLength
		{
			get
			{
				throw new NotImplementedException("Managed.TextFormatting.TextLineImp.get_TrailingWhitespaceLength");
			}
		}

		public override int Length
		{
			get
			{
				throw new NotImplementedException("Managed.TextFormatting.TextLineImp.get_Length");
			}
		}

		public override int DependentLength
		{
			get
			{
				throw new NotImplementedException("Managed.TextFormatting.TextLineImp.get_DependentLength");
			}
		}

		public override int NewlineLength
		{
			get
			{
				throw new NotImplementedException("Managed.TextFormatting.TextLineImp.get_NewlineLength");
			}
		}

		public override double Start
		{
			get
			{
				throw new NotImplementedException("Managed.TextFormatting.TextLineImp.get_Start");
			}
		}

		public override double Width
		{
			get
			{
				throw new NotImplementedException("Managed.TextFormatting.TextLineImp.get_Width");
			}
		}

		public override double WidthIncludingTrailingWhitespace
		{
			get
			{
				throw new NotImplementedException("Managed.TextFormatting.TextLineImp.get_WidthIncludingTrailingWhitespace");
			}
		}

		public override double Height
		{
			get
			{
				throw new NotImplementedException("Managed.TextFormatting.TextLineImp.get_Height");
			}
		}

		public override double TextHeight
		{
			get
			{
				throw new NotImplementedException("Managed.TextFormatting.TextLineImp.get_TextHeight");
			}
		}

		public override double Baseline
		{
			get
			{
				throw new NotImplementedException("Managed.TextFormatting.TextLineImp.get_Baseline");
			}
		}

		public override double TextBaseline
		{
			get
			{
				throw new NotImplementedException("Managed.TextFormatting.TextLineImp.get_TextBaseline");
			}
		}

		public override double MarkerBaseline
		{
			get
			{
				throw new NotImplementedException("Managed.TextFormatting.TextLineImp.get_MarkerBaseline");
			}
		}

		public override double MarkerHeight
		{
			get
			{
				throw new NotImplementedException("Managed.TextFormatting.TextLineImp.get_MarkerHeight");
			}
		}

		public override double Extent
		{
			get
			{
				throw new NotImplementedException("Managed.TextFormatting.TextLineImp.get_Extent");
			}
		}

		public override double OverhangLeading
		{
			get
			{
				throw new NotImplementedException("Managed.TextFormatting.TextLineImp.get_OverhangLeading");
			}
		}

		public override double OverhangTrailing
		{
			get
			{
				throw new NotImplementedException("Managed.TextFormatting.TextLineImp.get_OverhangTrailing");
			}
		}

		public override double OverhangAfter
		{
			get
			{
				throw new NotImplementedException("Managed.TextFormatting.TextLineImp.get_OverhangAfter");
			}
		}

		public override bool HasOverflowed
		{
			get
			{
				throw new NotImplementedException("Managed.TextFormatting.TextLineImp.get_HasOverflowed");
			}
		}

		public override bool HasCollapsed
		{
			get
			{
				throw new NotImplementedException("Managed.TextFormatting.TextLineImp.get_HasCollapsed");
			}
		}

		public override bool IsTruncated
		{
			get
			{
				throw new NotImplementedException("Managed.TextFormatting.TextLineImp.get_IsTruncated");
			}
		}
	}
}

