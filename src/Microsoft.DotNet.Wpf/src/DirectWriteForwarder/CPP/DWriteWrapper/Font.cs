// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;

namespace MS.Internal.Text.TextInterface
{
public sealed class Font
{
	/// <summary>
	/// The DWrite font object that this class wraps.
	/// </summary>
	IDWriteFont _font;

	/// <summary>
	/// The font's version number.
	/// </summary>
	double _version;

	/// <summary>
	/// FontMetrics for this font. Lazily allocated.
	/// </summary>
	FontMetrics? _fontMetrics;

	[Flags]
    internal enum FontFlags
    {
        VersionInitialized            = 0x0001,
        IsSymbolFontInitialized       = 0x0002,
        IsSymbolFontValue             = 0x0004,
    };

	/// <summary>
	/// Flags reflecting the state of the object.
	/// </summary>
	FontFlags _flags;

	/// <summary>
	/// Mutex used to control access to _fontFaceCache, which is locked when
	/// _mutex > 0.
	/// </summary>
	static int _mutex;

	/// <summary>
	/// Size of the _fontFaceCache, maximum number of FontFace instances cached.
	/// </summary>
	/// <remarks>
	/// Cache size could be based upon measurements of the TextFormatter micro benchmarks.
	/// English test cases allocate 1 - 3 FontFace instances, at the opposite extreme
	/// the Korean test maxes out at 13.  16 looks like a reasonable cache size.
	///
	/// However, dwrite circa win7 has an issue aggressively consuming address space and
	/// therefore we need to be conservative holding on to font references.
	/// 

	const int _fontFaceCacheSize = 4;

	struct FontFaceCacheEntry
	{
		internal Font font;
		internal FontFace fontFace;
	}

	/// <summary>
	/// Cached FontFace instances.
	/// </summary>
	static FontFaceCacheEntry[] _fontFaceCache;

	/// <summary>
	/// Most recently used element in the FontFace cache.
	/// </summary>
	static int _fontFaceCacheMRU;

    public Font(
      IDWriteFont font
      )
    {
        _font = font;
        _version = Double.MinValue;
        _flags = 0;
    }

    FontFace AddFontFaceToCache()
    {
        FontFace fontFace = CreateFontFace();
        FontFace bumpedFontFace = null;

        // NB: if the cache is busy, we simply return the new FontFace
        // without bothering to cache it.
        if (Interlocked.Increment(ref _mutex) == 1)
        {
            if (null == _fontFaceCache)
            {
                _fontFaceCache = new FontFaceCacheEntry[_fontFaceCacheSize];
            }
            
            // Default to a slot that is not the MRU.
            _fontFaceCacheMRU = (_fontFaceCacheMRU + 1) % _fontFaceCacheSize;

            // Look for an empty slot.
            for (int i=0; i < _fontFaceCacheSize; i++)
            {
                if (_fontFaceCache[i].font == null)
                {
                    _fontFaceCacheMRU = i;
                    break;
                }
            }

            // Keep a reference to any discarded entry, clean it up after releasing
            // the mutex.
            bumpedFontFace = _fontFaceCache[_fontFaceCacheMRU].fontFace;

            // Record the new entry.
            _fontFaceCache[_fontFaceCacheMRU].font = this;
            _fontFaceCache[_fontFaceCacheMRU].fontFace = fontFace;
            fontFace.AddRef();
        }
        Interlocked.Decrement(ref _mutex);

        // If the cache was full and we replaced an unreferenced entry, release it now.
        if (bumpedFontFace != null)
        {
            bumpedFontFace.Release();
        }

        return fontFace;
    }

    FontFace LookupFontFaceSlow()
    {
        FontFace fontFace = null;

        for (int i=0; i < _fontFaceCacheSize; i++)
        {
            if (_fontFaceCache[i].font == this)
            {
                fontFace = _fontFaceCache[i].fontFace;
                fontFace.AddRef();
                _fontFaceCacheMRU = i;
                break;
            }
        }

        return fontFace;
    }

    public static void ResetFontFaceCache()
    {
        FontFaceCacheEntry[] fontFaceCache = null;

        // NB: If the cache is busy, we do nothing.
        if (Interlocked.Increment(ref _mutex) == 1)
        {
            fontFaceCache = _fontFaceCache;
            _fontFaceCache = null;
        }
        Interlocked.Decrement(ref _mutex);

        if (fontFaceCache != null)
        {
            for (int i=0; i < _fontFaceCacheSize; i++)
            {
                if (fontFaceCache[i].fontFace != null)
                {
                    fontFaceCache[i].fontFace.Release();
                }
            }
        }
    }

    public FontFace GetFontFace()
    {
        FontFace fontFace = null;

        if (Interlocked.Increment(ref _mutex) == 1)
        {
            if (_fontFaceCache != null)
            {
                FontFaceCacheEntry entry;
                // Try the fast path first -- is caller accessing exactly the mru entry?
                if ((entry = _fontFaceCache[_fontFaceCacheMRU]).font == this)
                {
                    entry.fontFace.AddRef();
                    fontFace = entry.fontFace;
                }
                else
                {
                    // No luck, do a search through the cache.
                    fontFace = LookupFontFaceSlow();
                }
            }
        }
        Interlocked.Decrement(ref _mutex);

        // If the cache was busy or did not contain this Font, create a new FontFace.
        if (null == fontFace)
        {
            fontFace = AddFontFaceToCache();
        }

        return fontFace;
    }

    public IDWriteFont DWriteFont
    {
		get {
	        return _font;
		}
    }

/* requires FontFamily
    __declspec(noinline) FontFamily^ Font::Family::get()
    {
        IDWriteFontFamily* dwriteFontFamily;
        HRESULT hr = _font->Value->GetFontFamily(
                                         &dwriteFontFamily
                                         );
        System::GC::KeepAlive(_font);
        ConvertHresultToException(hr, "FontFamily^ Font::Family::get");
        return gcnew FontFamily(dwriteFontFamily);
    }
*/

    public FontWeight Weight
    {
		get {
			return _font.GetWeight();
		}
    }

    public FontStretch Stretch
    {
		get {
			return _font.GetStretch();
		}
	}

    public FontStyle Style
    {
		get {
			return _font.GetStyle();
		}
	}

    public bool IsSymbolFont
    {
		get {
			if ((_flags & FontFlags.IsSymbolFontInitialized) != FontFlags.IsSymbolFontInitialized)
			{
				bool isSymbolFont = _font.IsSymbolFont();
				_flags |= FontFlags.IsSymbolFontInitialized;
				if (isSymbolFont)
				{
					_flags |= FontFlags.IsSymbolFontValue;
				}
			}
			return ((_flags & FontFlags.IsSymbolFontValue) == FontFlags.IsSymbolFontValue);
		}
    }

    public LocalizedStrings FaceNames
    {
		get {
			IDWriteLocalizedStrings dwriteLocalizedStrings = _font.GetFaceNames();
			return new LocalizedStrings(dwriteLocalizedStrings);
		}
    }

    public bool GetInformationalStrings(InformationalStringID informationalStringID, [Out] out LocalizedStrings informationalStrings)
    {
        IDWriteLocalizedStrings dwriteInformationalStrings;
        bool exists = false;
		_font.GetInformationalStrings(informationalStringID, out dwriteInformationalStrings, out exists);
        informationalStrings = new LocalizedStrings(dwriteInformationalStrings);
        return exists;
    }

    public FontSimulations SimulationFlags
    {
		get {
			return _font.GetSimulations();
		}
	}

    public FontMetrics Metrics
    {
		get {
			if (_fontMetrics == null)
			{
				FontMetrics fontMetrics;
				_font.GetMetrics(out fontMetrics);
				_fontMetrics = fontMetrics;
			}
			return (FontMetrics)_fontMetrics;
		}
	}

    public bool HasCharacter(uint unicodeValue)
    {
        return _font.HasCharacter(unicodeValue);
    }

    FontFace CreateFontFace()
    {
        IntPtr dwriteFontFace = _font.CreateFontFace();
        return new FontFace(dwriteFontFace);
    }

    public double Version
    {
		get {
			if ((_flags & FontFlags.VersionInitialized) != FontFlags.VersionInitialized)
			{
				LocalizedStrings versionNumbers;
				double version = 0.0;
				if (GetInformationalStrings(InformationalStringID.VersionStrings, out versionNumbers))
				{
					string versionString = versionNumbers.GetString(0);

					// The following logic assumes that the version string is always formatted in this way: "Version X.XX"
					if(versionString.Length > 1)
					{
						versionString = versionString.Substring(versionString.LastIndexOf(' ') + 1);           
						if (!Double.TryParse(versionString, NumberStyles.Float, CultureInfo.InvariantCulture, out version))
						{
							version = 0.0;
						}
					}
				}
				_flags |= FontFlags.VersionInitialized;
				_version = version;
			}
			return _version;
		}
    }

/* requires Factory
    public FontMetrics DisplayMetrics(float emSize, float pixelsPerDip)
    {
		DWriteFontMetrics fontMetrics;
        IntPtr fontFace = _font.CreateFontFace();
		try {
			HRESULT hr = _font->Value->CreateFontFace(&fontFace);
			ConvertHresultToException(hr, "FontMetrics^ Font::DisplayMetrics");
			DWRITE_MATRIX transform = Factory::GetIdentityTransform();
			hr = fontFace->GetGdiCompatibleMetrics(
										emSize,
										pixelsPerDip,
										&transform,
										&fontMetrics
										); 
		}
		finally
		{
			Marshal.Release(fontFace);
		}

        ConvertHresultToException(hr, "FontMetrics^ Font::DisplayMetrics");
        System::GC::KeepAlive(_font);
        return DWriteTypeConverter::Convert(fontMetrics);
    }
*/
}
}
