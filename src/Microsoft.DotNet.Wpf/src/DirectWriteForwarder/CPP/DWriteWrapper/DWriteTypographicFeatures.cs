using System;
using System.Runtime.InteropServices;

namespace MS.Internal.Text.TextInterface
{
	[StructLayout (LayoutKind.Sequential)]
	internal struct DWriteTypographicFeatures
	{
		internal IntPtr features; /* DWRITE_FONT_FEATURE* */
		internal int featureCount;
	}
}
