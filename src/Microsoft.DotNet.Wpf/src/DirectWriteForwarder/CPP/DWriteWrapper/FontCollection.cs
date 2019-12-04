// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;

namespace MS.Internal.Text.TextInterface
{
public sealed class FontCollection
{
	IDWriteFontCollection _fontCollection;

    public FontCollection(IDWriteFontCollection fontCollection)
    {
        _fontCollection = fontCollection;
    }

    public bool FindFamilyName(string familyName, [Out] out uint index)
    {
        bool exists;
		_fontCollection.FindFamilyName(familyName, out index, out exists);
        return exists;
    }

    public Font GetFontFromFontFace(FontFace fontFace)
    {
        IntPtr dwriteFontFace = fontFace.DWriteFontFaceNoAddRef;
        IDWriteFont dwriteFont;
		try
		{
			dwriteFont = _fontCollection.GetFontFromFontFace(dwriteFontFace);
		}
		catch (Exception e)
		{
			if (e.HResult == unchecked((int)0x88985002)) /* DWRITE_E_NOFONT */
			{
				return null;
			}
			else
			{
				throw;
			}
		}
        return new Font(dwriteFont);
    }

    public FontFamily this[uint familyIndex]
    {
		get {
			IDWriteFontFamily dwriteFontFamily;

			dwriteFontFamily = _fontCollection.GetFontFamily(familyIndex);

			return new FontFamily(dwriteFontFamily);
		}
	}

    public FontFamily this[string familyName]
    {
		get {
			uint index;
			bool exists = FindFamilyName(familyName, out index);
			if (exists)
			{
				return this[index];
			}
			return null;
		}
	}

    public uint FamilyCount
    {
		get {
			return _fontCollection.GetFontFamilyCount();
		}
    }
}
}
