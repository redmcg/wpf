using System;
using System.Runtime.InteropServices;

namespace MS.Internal.Text.TextInterface
{
	internal unsafe class TextAnalyzerSource : IDWriteTextAnalysisSource
	{
		char* pText;
		uint TextLength;
		IDWriteNumberSubstitution NumberSubstitution;
		GCHandle PinnedLocaleName;
		DWriteReadingDirection ReadingDirection;

		internal TextAnalyzerSource(char* text, uint length, string culture,
			IDWriteFactory factory, bool isRightToLeft, string numberCulture,
			bool ignoreUserOverride, uint numberSubstitutionMethod)
		{
			pText = text;
			TextLength = length;
			NumberSubstitution = factory.CreateNumberSubstitution(numberSubstitutionMethod, numberCulture, ignoreUserOverride);
			PinnedLocaleName = GCHandle.Alloc(culture, GCHandleType.Pinned);
			ReadingDirection = isRightToLeft ? DWriteReadingDirection.RightToLeft : DWriteReadingDirection.LeftToRight;
		}

		public void GetTextAtPosition(uint position, out IntPtr text, out uint text_len)
		{
			if (position < 0 || position >= TextLength)
			{
				text = IntPtr.Zero;
				text_len = 0;
			}
			else
			{
				text = new IntPtr(pText + position);
				text_len = TextLength - position;
			}
		}

		public void GetTextBeforePosition(uint position, out IntPtr text, out uint text_len)
		{
			if (position <= 0 || position > TextLength)
			{
				text = IntPtr.Zero;
				text_len = 0;
			}
			else
			{
				text = new IntPtr(pText);
				text_len = position;
			}
		}

		public DWriteReadingDirection GetParagraphReadingDirection()
		{
			return ReadingDirection;
		}

		public void GetLocaleName(uint position, out uint text_len, out IntPtr locale)
		{
			text_len = TextLength - position;
			locale = PinnedLocaleName.AddrOfPinnedObject();
		}

		public void GetNumberSubstitution(uint position, out uint text_len, out IDWriteNumberSubstitution substitution)
		{
			text_len = TextLength - position;
			substitution = NumberSubstitution;
		}
	}
}
