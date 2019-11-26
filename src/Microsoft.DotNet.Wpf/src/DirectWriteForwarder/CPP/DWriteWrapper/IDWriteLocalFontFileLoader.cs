using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MS.Internal.Text.TextInterface
{
	[Guid ("b2d9f3ec-c9fe-4a11-a2ec-d86208f7c0a2")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	[ComImport ()]
	internal interface IDWriteLocalFontFileLoader
	{
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		uint GetFilePathLengthFromKey(
			[In] IntPtr key, /* void const* */
			[In] uint key_size);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void GetFilePathFromKey(
			[In] IntPtr key, /* void const* */
			[In] uint key_size,
			[In] IntPtr path, /* WCHAR* */
			[In] uint length);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void GetLastWriteTypeFromKey(
			[In] IntPtr key, /* void const* */
			[In] uint key_size,
			[In] IntPtr writetime); /* out FILETIME* */
	}
}
