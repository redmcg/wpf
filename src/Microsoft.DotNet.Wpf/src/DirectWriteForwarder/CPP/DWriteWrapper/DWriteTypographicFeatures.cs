using System;
using System.Runtime.InteropServices;

namespace MS.Internal.Text.TextInterface
{
	[StructLayout (LayoutKind.Sequential)]
	internal struct DWriteTypographicFeatures
	{
		IntPtr features; /* DWRITE_FONT_FEATURE* */
		uint featureCount;
	}
}
