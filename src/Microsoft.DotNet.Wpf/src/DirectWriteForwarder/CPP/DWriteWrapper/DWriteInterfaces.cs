using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MS.Internal.Text.TextInterface
{
	[Guid ("6d4865fe-0ab8-4d91-8f62-5dd6be34a3e0")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	[ComImport ()]
	internal interface IDWriteFontFileStreamMirror
	{
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[PreserveSig]
		public int ReadFileFragment(
			[Out] out IntPtr fragment_start,
			[In] ulong offset,
			[In] ulong fragment_size,
			[Out] out IntPtr fragment_context);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[PreserveSig]
		public void ReleaseFileFragment(
			[In] IntPtr fragment_context);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[PreserveSig]
		public int GetFileSize(
			[Out] out ulong size);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[PreserveSig]
		public int GetLastWriteTime(
			[Out] out ulong lastWriteTime);
	}

	[Guid ("727cad4e-d6af-4c9e-8a08-d695b11caa49")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	[ComImport ()]
	internal interface IDWriteFontFileLoaderMirror
	{
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[PreserveSig]
		public int CreateStreamFromKey(
			[In] IntPtr key, /* const void* */
			[In] uint key_size,
			[Out, MarshalAs(UnmanagedType.Interface)] out IDWriteFontFileStreamMirror stream);
	}

	[Guid ("72755049-5ff7-435d-8348-4be97cfa6c7c")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	[ComImport ()]
	internal interface IDWriteFontFileEnumeratorMirror
	{
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[PreserveSig]
		public int MoveNext(
			[Out] bool has_current_file);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[PreserveSig]
		public int GetCurrentFontFile(
			[Out, MarshalAs(UnmanagedType.Interface)] out IDWriteFontFile font_file);
	}

	[Guid ("cca920e4-52f0-492b-bfa8-29c72ee0a468")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	[ComImport ()]
	internal interface IDWriteFontCollectionLoaderMirror
	{
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[PreserveSig]
		public int CreateEnumeratorFromKey(
			[In] IntPtr factory, /* IDWriteFactory* */
			[In] IntPtr key, /* void const* */
			[In] uint key_size,
			[Out] out IDWriteFontFileEnumeratorMirror enumerator);
	}
}
