using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MS.Internal.Text.TextInterface
{
	[Guid ("b7e6163e-7f46-43b4-84b3-e4e6249c365d")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	[ComImport ()]
	internal interface IDWriteTextAnalyzer
	{
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void AnalyzeScript(
			[In, MarshalAs(UnmanagedType.Interface)] IDWriteTextAnalysisSource source,
			uint position,
			uint length,
			[In, MarshalAs(UnmanagedType.Interface)] IDWriteTextAnalysisSink sink);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void AnalyzeBidi(
			[In, MarshalAs(UnmanagedType.Interface)] IDWriteTextAnalysisSource source,
			uint position,
			uint length,
			[In, MarshalAs(UnmanagedType.Interface)] IDWriteTextAnalysisSink sink);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void AnalyzeNumberSubstitution(
			[In, MarshalAs(UnmanagedType.Interface)] IDWriteTextAnalysisSource source,
			uint position,
			uint length,
			[In, MarshalAs(UnmanagedType.Interface)] IDWriteTextAnalysisSink sink);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void AnalyzeLineBreakpoints(
			[In, MarshalAs(UnmanagedType.Interface)] IDWriteTextAnalysisSource source,
			uint position,
			uint length,
			[In, MarshalAs(UnmanagedType.Interface)] IDWriteTextAnalysisSink sink);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		unsafe void GetGlyphs(
			IntPtr text, /* const WCHAR* */
			uint length,
			IntPtr font_face, /* IDWriteFontFace* */
			bool is_sideways,
			bool is_rtl,
			[In] ref DWriteScriptAnalysis analysis, /* in */
			[In, MarshalAs(UnmanagedType.LPWStr)] string locale, 
			[In, MarshalAs(UnmanagedType.Interface)] IDWriteNumberSubstitution substitution,
			[In] IntPtr features, /* DWRITE_TYPOGRAPHIC_FEATURES const ** */
			[In, MarshalAs(UnmanagedType.LPArray)] uint[] feature_range_len,
			uint feature_ranges,
			uint max_glyph_count,
			ushort* clustermap,
			ushort* text_props, /* DWRITE_SHAPING_TEXT_PROPERTIES* */
			ushort* glyph_indices,
			ushort* glyph_props, /* DWRITE_SHAPING_GLYPH_PROPERTIES */
			out uint actual_glyph_count);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		unsafe void GetGlyphPlacements(
			IntPtr text, /* const WCHAR* */
			ushort* clustermap,
			ushort* props, /* DWRITE_SHAPING_TEXT_PROPERTIES */
			uint text_len,
			ushort* glyph_indices,
			ushort* glyph_props, /* DWRITE_SHAPING_GLYPH_PROPERTIES */
			uint glyph_count,
			IntPtr font_face, /* IDWriteFontFace */
			float fontEmSize,
			bool is_sideways,
			bool is_rtl,
			[In] ref DWriteScriptAnalysis analysis,
			[In, MarshalAs(UnmanagedType.LPWStr)] string locale, 
			[In] IntPtr features, /* DWRITE_TYPOGRAPHIC_FEATURES const ** */
			[In, MarshalAs(UnmanagedType.LPArray)] uint[] feature_range_len,
			uint feature_ranges,
			float* glyph_advances,
			[Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=6)] out DWriteGlyphOffset glyph_offsets);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		unsafe void GetGdiCompatibleGlyphPlacements(
			IntPtr text, /* const WCHAR* */
			ushort* clustermap,
			ushort* props, /* DWRITE_SHAPING_TEXT_PROPERTIES */
			uint text_len,
			ushort* glyph_indices,
			ushort* glyph_props, /* DWRITE_SHAPING_GLYPH_PROPERTIES */
			uint glyph_count,
			IntPtr font_face, /* IDWriteFontFace */
			float fontEmSize,
			float pixels_per_dip,
			[In] ref DWriteMatrix transform,
			bool use_gdi_natural,
			bool is_sideways,
			bool is_rtl,
			[In] ref DWriteScriptAnalysis analysis,
			[In, MarshalAs(UnmanagedType.LPWStr)] string locale, 
			[In] IntPtr features, /* DWRITE_TYPOGRAPHIC_FEATURES const ** */
			[In, MarshalAs(UnmanagedType.LPArray)] uint[] feature_range_lengths,
			uint feature_ranges,
			float* glyph_advances,
			[Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=6)] out DWriteGlyphOffset glyph_offsets);
	}
}
