using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MS.Internal.Text.TextInterface
{
	[Guid ("08256209-099a-4b34-b86d-c22b110e7771")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	[ComImport ()]
	public interface IDWriteLocalizedStrings
	{
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[PreserveSig]
		uint GetCount();

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void FindLocaleName(
			[In, MarshalAs(UnmanagedType.LPWStr)] string locale_name,
			[Out] out uint index,
			[Out] out bool exists);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		uint GetLocaleNameLength(
			[In] uint index);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void GetLocaleName(
			[In] uint index,
			[In] IntPtr locale_name, /* out WCHAR* */
			[In] uint size);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		uint GetStringLength(
			[In] uint index);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void GetString(
			[In] uint index,
			[In] IntPtr buffer, /* out WCHAR* */
			[In] uint size);
	}
}
