// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;

namespace MS.Internal.Text.TextInterface
{
public sealed class FontFile
{
	IDWriteFontFile _fontFile;

    internal FontFile(IDWriteFontFile fontFile)
    {
        _fontFile = fontFile;
    }

    internal bool Analyze(
		[Out] out FontFileType fontFileType,
		[Out] out FontFaceType fontFaceType,
		[Out] out uint numberOfFaces)
	{
        bool isSupported = false;
		_fontFile.Analyze(out isSupported, out fontFileType, out fontFaceType, out numberOfFaces);
        return isSupported;
    }

    internal string GetUriPath()
    {
        IntPtr fontFileReferenceKey;
        uint sizeOfFontFileReferenceKey;

        var fontFileLoader = _fontFile.GetLoader();

		var localFontFileLoader = fontFileLoader as IDWriteLocalFontFileLoader;
		if (localFontFileLoader == null)
		{
			// No reason to expect this to be a valid wchar string.
			//_fontFile.GetReferenceKey(out fontFileReferenceKey, out sizeOfFontFileReferenceKey);
			//return Marshal.PtrToStringUni(fontFileReferenceKey);
			throw new InvalidOperationException("FontFile not loaded from local file");
        }
        else
        {
			_fontFile.GetReferenceKey(out fontFileReferenceKey, out sizeOfFontFileReferenceKey);

			uint sizeOfFilePath = localFontFileLoader.GetFilePathLengthFromKey(
														   fontFileReferenceKey,
														   sizeOfFontFileReferenceKey);

			// How is it possible for this to be FALSE??
			// MS::Internal::Invariant::Assert(sizeOfFilePath >= 0 && sizeOfFilePath < UINT_MAX);

			IntPtr fontFilePath = Marshal.AllocCoTaskMem(2 * ((int)sizeOfFilePath + 1));
			try
			{
				localFontFileLoader.GetFilePathFromKey(
															fontFileReferenceKey,
															sizeOfFontFileReferenceKey,
															fontFilePath,
															sizeOfFilePath + 1
															);
				return Marshal.PtrToStringUni(fontFilePath);
			}
			finally
			{
				Marshal.FreeCoTaskMem(fontFilePath);
			}
        }
    }
}
}
