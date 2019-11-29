using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MS.Internal.Text.TextInterface
{
	[Guid ("acd16696-8c14-4f5d-877e-fe3fc1d32737")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	[ComImport ()]
	internal interface IDWriteFont
	{
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		IntPtr GetFontFamily(); /* returns IDWriteFontFamily */

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[PreserveSig]
		FontWeight GetWeight();

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[PreserveSig]
		FontStretch GetStretch();

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[PreserveSig]
		FontStyle GetStyle();

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[PreserveSig]
		bool IsSymbolFont();

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs(UnmanagedType.Interface)]
		IDWriteLocalizedStrings GetFaceNames();

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void GetInformationalStrings(
			InformationalStringID stringid,
			[Out, MarshalAs(UnmanagedType.Interface)] out IDWriteLocalizedStrings strings,
			[Out] out bool exists);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[PreserveSig]
		FontSimulations GetSimulations();

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[PreserveSig]
		void GetMetrics([Out] out FontMetrics metrics);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		bool HasCharacter(uint value);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		IntPtr CreateFontFace(); /* returns IDWriteFontFace* */
	}
}
