using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MS.Internal.Text.TextInterface
{
	[Guid ("688e1a58-5094-47c8-adc8-fbcea60ae92b")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	[ComImport ()]
	internal unsafe interface IDWriteTextAnalysisSource
	{
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void GetTextAtPosition(
			uint position,
			[Out] out IntPtr text, /* WCHAR const** */
			[Out] out uint text_len);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void GetTextBeforePosition(
			uint position,
			[Out] out IntPtr text, /* WCHAR const** */
			[Out] out uint text_len);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[PreserveSig]
		DWriteReadingDirection GetParagraphReadingDirection();

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void GetLocaleName(
			uint position,
			[Out] out uint text_len,
			[Out] IntPtr locale); /* WCHAR const** */

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void GetNumberSubstitution(
			uint position,
			[Out] out uint text_len,
			[Out, MarshalAs(UnmanagedType.Interface)] IDWriteNumberSubstitution substitution);
	}
}
