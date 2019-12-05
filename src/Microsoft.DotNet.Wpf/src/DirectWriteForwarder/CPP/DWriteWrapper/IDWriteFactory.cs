using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MS.Internal.Text.TextInterface
{
	[Guid ("b859ee5a-d838-4b5b-a2e8-1adc7d93db48")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	[ComImport ()]
	public interface IDWriteFactory
	{
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void GetSystemFontCollection(
			[Out, MarshalAs(UnmanagedType.Interface)] out IDWriteFontCollection collection,
			bool check_for_updates);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs(UnmanagedType.Interface)]
		IDWriteFontCollection CreateCustomFontCollection(
			[MarshalAs(UnmanagedType.Interface)] IDWriteFontCollectionLoaderMirror loader,
			IntPtr key, // void const *
			uint key_size);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void RegisterFontCollectionLoader(IDWriteFontCollectionLoaderMirror loader);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void UnregisterFontCollectionLoader(IDWriteFontCollectionLoaderMirror loader);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs(UnmanagedType.Interface)]
		IDWriteFontFile CreateFontFileReference(
			[In, MarshalAs(UnmanagedType.LPWStr)] string path,
			IntPtr writetime); // FILETIME const *

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs(UnmanagedType.Interface)]
		IDWriteFontFile CreateCustomFontFileReference(
			IntPtr reference_key,
			uint key_size,
			[MarshalAs(UnmanagedType.Interface)] IDWriteFontFileLoaderMirror loader);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		IntPtr CreateFontFace( // returns IDWriteFontFace*
			FontFaceType facetype,
			uint files_number,
			[In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] IntPtr[] font_files, // array of IDWriteFontFile*
			uint index,
			FontSimulations sim_flags);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		IntPtr CreateRenderingParams(); // returns IDWriteRenderingParams*

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		IntPtr CreateMonitorRenderingParams( // returns IDWriteRenderingParams*
			IntPtr monitor); // HMONITOR

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		IntPtr CreateCustomRenderingParams( // returns IDWriteRenderingParams*
			float gamma,
			float enhancedContrast,
			float cleartype_level,
			int geometry, // DWRITE_PIXEL_GEOMETRY 
			int mode); // DWRITE_RENDERING_MODE

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void RegisterFontFileLoader([MarshalAs(UnmanagedType.Interface)] IDWriteFontFileLoaderMirror loader);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void UnregisterFontFileLoader([MarshalAs(UnmanagedType.Interface)] IDWriteFontFileLoaderMirror loader);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		IntPtr CreateTextFormat( // returns IDWriteTextFormat*
			[In, MarshalAs(UnmanagedType.LPWStr)] string family_name,
			[In, MarshalAs(UnmanagedType.Interface)] IDWriteFontCollection collection,
			FontWeight weight,
			FontStyle style,
			FontStretch stretch,
			float size,
			[In, MarshalAs(UnmanagedType.LPWStr)] string locale);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		IntPtr CreateTypography(); // returns IDWriteTypography*

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		IntPtr GetGdiInterop(); // returns IDWriteGdiInterop*

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		IntPtr CreateTextLayout( // returns IDWriteTextLayout*
			[In, MarshalAs(UnmanagedType.LPWStr)] string str,
			uint len,
			IntPtr format, // IDWriteTextFormat*
			float max_width,
			float max_height);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		IntPtr CreateGdiCompatibleTextLayout( // returns IDWriteTextLayout*
			[In, MarshalAs(UnmanagedType.LPWStr)] string str,
			uint len,
			IntPtr format, // IDWriteTextFormat*
			float layout_width,
			float layout_height,
			float pixels_per_dip,
			[In] ref DWriteMatrix transform,
			bool use_gdi_natural);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		IntPtr CreateEllipsisTrimmingSign( // returns IDWriteInlineObject*
			IntPtr format); // IDWriteTextFormat

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs(UnmanagedType.Interface)]
		IDWriteTextAnalyzer CreateTextAnalyzer();

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs(UnmanagedType.Interface)]
		IDWriteNumberSubstitution CreateNumberSubstitution(
			int method, // DWRITE_NUMBER_SUBSTITUTION_METHOD
			[In, MarshalAs(UnmanagedType.LPWStr)] string locale,
			bool ignore_user_override);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		IntPtr CreateGlyphRunAnalysis( // IDWriteGlyphRunAnalysis*
			IntPtr glyph_run, // DWRITE_GLYPH_RUN const *
			float pixels_per_dip,
			[In] ref DWriteMatrix transform,
			int rendering_mode, // DWRITE_RENDERING_MODE
			int measuring_mode, // DWRITE_MEASURING_MODE
			float baseline_x,
			float baseline_y);
	}
}
