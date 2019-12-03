// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace MS.Internal.Text.TextInterface
{
internal sealed class FontFace : IDisposable
{
	IntPtr _fontFace; // IDWriteFontFace*
	FontMetrics _fontMetrics;
	bool _fontMetricsInitialized = false;
	int _refCount;

    internal FontFace(IntPtr fontFace)
    {
		Marshal.AddRef(fontFace);
        _fontFace = fontFace;
    }

	/// <summary>
	/// Increments the reference count on this FontFace.
	/// </summary>
	public void AddRef()
	{
		Interlocked.Increment(ref _refCount);
	}

	/// <summary>
	/// Decrements the reference count on this FontFace.
	/// </summary>
	public void Release()
	{
		if (-1 == Interlocked.Decrement(ref _refCount))
		{
			// At this point we know the FontFace is eligable for the finalizer queue.
			// However, because native dwrite font faces consume enormous amounts of 
			// address space, we need to be aggressive about disposing immediately.
			// If we rely solely on the GC finalizer, we will exhaust available address
			// space in reasonable scenarios like enumerating all installed fonts.
			Dispose();
		}
	}

	~FontFace()
	{
		Dispose();
	}

	public void Dispose()
	{
		if (_fontFace != IntPtr.Zero)
		{
			Marshal.ReleaseComObject(_fontFace);
			_fontFace = IntPtr.Zero;
		}
	}

    /// WARNING: AFTER GETTING THIS NATIVE POINTER YOU ARE RESPONSIBLE FOR MAKING SURE THAT THE WRAPPING MANAGED
    /// OBJECT IS KEPT ALIVE BY THE GC OR ELSE YOU ARE RISKING THE POINTER GETTING RELEASED BEFORE YOU'D 
    /// WANT TO.
    ///
    public IntPtr DWriteFontFaceNoAddRef
    {
		get {
	        return _fontFace;
		}
    }

    public IntPtr DWriteFontFaceAddRef
    {
		get {
			Marshal.AddRef(_fontFace);
			return _fontFace;
		}
    }

	TDelegate GetFunctionFromVTable<TDelegate>(int index)
	{
		IntPtr vtable = Marshal.ReadIntPtr(_fontFace);
		IntPtr fn = Marshal.ReadIntPtr(vtable, index * IntPtr.Size);
		return Marshal.GetDelegateForFunctionPointer<TDelegate>(fn);
	}

	[UnmanagedFunctionPointer (CallingConvention.StdCall)]
	delegate FontFaceType Delegate_GetType(IntPtr iface);

	public FontFaceType Type
	{
		get {
			var fn = GetFunctionFromVTable<Delegate_GetType>(3);
			return fn(_fontFace);
		}
	}

	[UnmanagedFunctionPointer (CallingConvention.StdCall)]
	delegate int Delegate_GetFiles(IntPtr iface,
		[In] [Out] ref uint number_of_files,
		IntPtr fontfiles); /* IDWriteFontFile** */

    public FontFile GetFileZero()
    {
        uint numberOfFiles = 0;
        IntPtr pfirstDWriteFontFile = IntPtr.Zero;
        IntPtr ppDWriteFontFiles = IntPtr.Zero;
		var GetFilesFn = GetFunctionFromVTable<Delegate_GetFiles>(4);

        // This first call is to retrieve the numberOfFiles.
		int hr = GetFilesFn(_fontFace, ref numberOfFiles, IntPtr.Zero);
		Marshal.ThrowExceptionForHR(hr);

        if (numberOfFiles == 0)
			return null;

		ppDWriteFontFiles = Marshal.AllocCoTaskMem(IntPtr.Size * (int)numberOfFiles);

		try
		{
			hr = GetFilesFn(_fontFace, ref numberOfFiles, ppDWriteFontFiles);
			Marshal.ThrowExceptionForHR(hr);

			pfirstDWriteFontFile = Marshal.ReadIntPtr(ppDWriteFontFiles, 0);

			for(uint i = 1; i < numberOfFiles; ++i)
				Marshal.Release(Marshal.ReadIntPtr(ppDWriteFontFiles, (int)i * IntPtr.Size));
		}
		finally
		{
			Marshal.FreeCoTaskMem(ppDWriteFontFiles);
		}

		var fontFile = Marshal.GetObjectForIUnknown(pfirstDWriteFontFile);
		Marshal.Release(pfirstDWriteFontFile);

        return new FontFile((IDWriteFontFile)fontFile);
    }

	[UnmanagedFunctionPointer (CallingConvention.StdCall)]
	delegate uint Delegate_GetIndex(IntPtr iface);

	public uint Index
    {
		get {
			var GetIndexFn = GetFunctionFromVTable<Delegate_GetIndex>(5);
			return GetIndexFn(_fontFace);
		}
	}

	[UnmanagedFunctionPointer (CallingConvention.StdCall)]
	delegate FontSimulations Delegate_GetSimulations(IntPtr iface);

	public FontSimulations SimulationFlags
	{
		get {
			var GetSimulationsFn = GetFunctionFromVTable<Delegate_GetSimulations>(6);
			return GetSimulationsFn(_fontFace);
		}
	}

	[UnmanagedFunctionPointer (CallingConvention.StdCall)]
	delegate bool Delegate_IsSymbolFont(IntPtr iface);

    public bool IsSymbolFont
	{
		get {
			var IsSymbolFontFn = GetFunctionFromVTable<Delegate_IsSymbolFont>(7);
			return IsSymbolFontFn(_fontFace);
		}
	}

	[UnmanagedFunctionPointer (CallingConvention.StdCall)]
	unsafe delegate void Delegate_GetMetrics(IntPtr iface, FontMetrics* metrics);

    public FontMetrics Metrics
    {
		get {
			if (!_fontMetricsInitialized)
			{
				unsafe
				{
					fixed (FontMetrics* p = &this._fontMetrics)
					{
						var GetMetricsFn = GetFunctionFromVTable<Delegate_GetMetrics>(8);
						GetMetricsFn(_fontFace, p);
						_fontMetricsInitialized = true;
					}
				}
			}
			return _fontMetrics;
		}
	}

	[UnmanagedFunctionPointer (CallingConvention.StdCall)]
	delegate ushort Delegate_GetGlyphCount(IntPtr iface);

    public ushort GlyphCount
    {
		get {
			var GetGlyphCountFn = GetFunctionFromVTable<Delegate_GetGlyphCount>(9);
			return GetGlyphCountFn(_fontFace);
		}
    }

	[UnmanagedFunctionPointer (CallingConvention.StdCall)]
	unsafe delegate int Delegate_GetDesignGlyphMetrics(IntPtr iface,
		ushort* glyph_indices,
		uint glyph_count,
		GlyphMetrics* metrics,
		bool is_sideways);

    public unsafe void GetDesignGlyphMetrics(ushort* pGlyphIndices, ushort glyphCount, GlyphMetrics* pGlyphMetrics)
    {
		var GetDesignGlyphMetricsFn = GetFunctionFromVTable<Delegate_GetDesignGlyphMetrics>(10);
		int hr = GetDesignGlyphMetricsFn(_fontFace, pGlyphIndices, glyphCount, pGlyphMetrics, false);
		Marshal.ThrowExceptionForHR(hr);
    }

	[UnmanagedFunctionPointer (CallingConvention.StdCall)]
	unsafe delegate int Delegate_GetGdiCompatibleGlyphMetrics(IntPtr iface,
		float emSize,
		float pixels_per_dip,
		IntPtr transform, /* DWRITE_MATRIX const* */
		bool use_gdi_natural,
		ushort *glyph_indices,
		uint glyph_count,
		GlyphMetrics *metrics,
		bool is_sideways);

    public unsafe void GetDisplayGlyphMetrics(
		ushort *pGlyphIndices,
		uint glyphCount,
		GlyphMetrics* pGlyphMetrics,
		float emSize,
        bool useDisplayNatural,
        bool isSideways,
        float pixelsPerDip)
    {
		var GetGdiCompatibleGlyphMetricsFn = GetFunctionFromVTable<Delegate_GetGdiCompatibleGlyphMetrics>(17);
		int hr = GetGdiCompatibleGlyphMetricsFn(_fontFace,
            emSize,
            pixelsPerDip, //FLOAT pixelsPerDip,
            IntPtr.Zero,
            useDisplayNatural, //BOOL useGdiNatural,
            pGlyphIndices,//__in_ecount(glyphCount) UINT16 const* glyphIndices,
            glyphCount, //UINT32 glyphCount,
            pGlyphMetrics, //__out_ecount(glyphCount) DWRITE_GLYPH_METRICS* glyphMetrics
            isSideways //BOOL isSideways,
            );         
		Marshal.ThrowExceptionForHR(hr);
    }

	[UnmanagedFunctionPointer (CallingConvention.StdCall)]
	unsafe delegate int Delegate_GetGlyphIndices(IntPtr iface,
		uint *codepoints,
		uint count,
		ushort *glyph_indices);

    public unsafe void GetArrayOfGlyphIndices(
        uint* pCodePoints,
        uint glyphCount,
        ushort* pGlyphIndices)
    {
		var GetGlyphIndicesFn = GetFunctionFromVTable<Delegate_GetGlyphIndices>(11);
        int hr = GetGlyphIndicesFn(_fontFace, pCodePoints, glyphCount, pGlyphIndices);
        Marshal.ThrowExceptionForHR(hr);
    }

	[UnmanagedFunctionPointer (CallingConvention.StdCall)]
	delegate int Delegate_TryGetFontTable(IntPtr iface,
        uint table_tag,
        [Out] out IntPtr table_data,
        [Out] out uint table_size,
        [Out] out IntPtr context,
    	[Out] out bool exists);

	[UnmanagedFunctionPointer (CallingConvention.StdCall)]
	delegate int Delegate_ReleaseFontTable(IntPtr iface, IntPtr table_context);

    public bool TryGetFontTable(OpenTypeTableTag openTypeTableTag, [Out] out byte[] tableData)
    {
        IntPtr tableDataDWrite;
        IntPtr tableContext;
        uint tableSizeDWrite = 0;
        bool exists = false;
        tableData = null;
		var TryGetFontTableFn = GetFunctionFromVTable<Delegate_TryGetFontTable>(12);
		var ReleaseFontTableFn = GetFunctionFromVTable<Delegate_ReleaseFontTable>(13);
		int hr = TryGetFontTableFn(_fontFace, (uint)openTypeTableTag,
                                              out tableDataDWrite,
                                              out tableSizeDWrite,
                                              out tableContext,
                                              out exists
                                              );
        Marshal.ThrowExceptionForHR(hr);

        if (exists)
        {
            tableData = new byte[tableSizeDWrite];
			Marshal.Copy(tableDataDWrite, tableData, 0, (int)tableSizeDWrite);
            
			ReleaseFontTableFn(_fontFace, tableContext);
        }
        return exists;
    }

    public bool ReadFontEmbeddingRights([Out] out ushort fsType)
    {
        IntPtr os2Table;
        IntPtr tableContext;
        uint tableSizeDWrite = 0;
        bool exists = false;
        fsType = 0;
		var TryGetFontTableFn = GetFunctionFromVTable<Delegate_TryGetFontTable>(12);
		var ReleaseFontTableFn = GetFunctionFromVTable<Delegate_ReleaseFontTable>(13);
		int hr = TryGetFontTableFn(_fontFace, (uint)OpenTypeTableTag.OS_2,
											out os2Table,
                                            out tableSizeDWrite,
                                            out tableContext,
                                            out exists
                                            );
		Marshal.ThrowExceptionForHR(hr);

        const int OFFSET_OS2_fsType = 8;
        bool success = false;
        if (exists)
        {
            if (tableSizeDWrite >= OFFSET_OS2_fsType + 1)
            {
				fsType = (ushort)((Marshal.ReadByte(os2Table, OFFSET_OS2_fsType) << 8) +
					Marshal.ReadByte(os2Table, OFFSET_OS2_fsType + 1));
                success = true;
            }
            
			ReleaseFontTableFn(_fontFace, tableContext);
        }
        
        return success; 
    }
}
}
