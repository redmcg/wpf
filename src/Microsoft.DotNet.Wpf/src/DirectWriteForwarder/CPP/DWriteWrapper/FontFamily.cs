// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace MS.Internal.Text.TextInterface
{
public class FontFamily : FontList
{
	IDWriteFontFamily _fontFamily;
	Font _regularFont;

    public FontFamily(IDWriteFontFamily fontFamily) : base((IDWriteFontList)fontFamily)
    {
		_fontFamily = fontFamily;
        _regularFont = null;
    }

    public LocalizedStrings FamilyNames
    {
		get {
			IDWriteLocalizedStrings dwriteLocalizedStrings;
			dwriteLocalizedStrings = _fontFamily.GetFamilyNames();
			return new LocalizedStrings(dwriteLocalizedStrings);
		}
	}

    public bool IsPhysical
    {
		get {
			return true;
		}
    }

    public bool IsComposite
    {
		get {
        	return false;
		}
    }

    public string OrdinalName
    {
		get {
			if (FamilyNames.StringsCount > 0)
			{
				return FamilyNames.GetString(0);
			}
			return String.Empty;
		}
    }

    public FontMetrics Metrics
    {
		get {
			if (_regularFont == null)
			{
				_regularFont = GetFirstMatchingFont(FontWeight.Normal, FontStretch.Normal, FontStyle.Normal);
			}
			return _regularFont.Metrics;
		}
    }

/* requires Factory
    public FontMetrics DisplayMetrics(float emSize, float pixelsPerDip)
    {
        Font regularFont = GetFirstMatchingFont(FontWeight.Normal, FontStretch.Normal, FontStyle.Normal);
        return regularFont.DisplayMetrics(emSize, pixelsPerDip);
    }
*/

    public Font GetFirstMatchingFont(FontWeight weight, FontStretch stretch, FontStyle style)
    {
        IDWriteFont dwriteFont;

		dwriteFont = _fontFamily.GetFirstMatchingFont(weight, stretch, style);
        return new Font(dwriteFont);
    }

    public FontList GetMatchingFonts(FontWeight weight, FontStretch stretch, FontStyle style)
    {
        IDWriteFontList dwriteFontList;
		dwriteFontList = _fontFamily.GetMatchingFonts(weight, stretch, style);
        return new FontList(dwriteFontList);
    }
}
}
