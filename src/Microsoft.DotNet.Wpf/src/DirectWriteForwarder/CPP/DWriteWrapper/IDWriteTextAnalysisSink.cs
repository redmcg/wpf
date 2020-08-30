using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MS.Internal.Text.TextInterface
{
	[Guid ("5810cd44-0ca0-4701-b3fa-bec5182ae4f6")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	[ComImport ()]
	public interface IDWriteTextAnalysisSink
	{
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void SetScriptAnalysis(
			uint position,
			uint length,
			ref DWriteScriptAnalysis scriptanalysis);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void SetLineBreakpoints(
			uint position,
			uint length,
			IntPtr breakpoints); /* DWRITE_LINE_BREAKPOINT const* */

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void SetBidiLevel(
			uint position,
			uint length,
			byte explicitLevel,
			byte resolvedLevel);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void SetNumberSubstitution(
			uint position,
			uint length,
			[In, MarshalAs(UnmanagedType.Interface)] IDWriteNumberSubstitution substitution);
	}
}
