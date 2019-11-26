using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MS.Internal.Text.TextInterface
{
	[Guid ("739d886a-cef5-47dc-8769-1a8b41bebbb0")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	[ComImport ()]
	internal interface IDWriteFontFile
	{
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void GetReferenceKey(
			[Out] out IntPtr key, /* void const ** */
			[Out] out uint key_size);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs(UnmanagedType.Interface)]
		IDWriteFontFileLoaderMirror GetLoader();

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void Analyze(
			[Out] out bool is_supported_fonttype,
			[Out] out FontFileType file_type,
			[Out] out FontFaceType face_type,
			[Out] out uint faces_num);
	}
}
