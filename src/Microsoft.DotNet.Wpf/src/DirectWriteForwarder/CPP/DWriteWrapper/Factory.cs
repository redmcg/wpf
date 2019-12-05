using System;
using System.Runtime.InteropServices;

namespace MS.Internal.Text.TextInterface
{
	public sealed class Factory
	{
		public static int CreateFontFile(
			IDWriteFactory factory,
			FontFileLoader fontFileLoader,
			Uri filePathUri,
			out IDWriteFontFile dwriteFontFile)
		{
			throw new NotImplementedException();
		}
	}
}
