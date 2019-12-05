// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace MS.Internal.Text.TextInterface
{
    [ClassInterface(ClassInterfaceType.None), ComVisible(true)]
    public class FontFileLoader : IDWriteFontFileLoaderMirror
    {
        IFontSourceFactory _fontSourceFactory;

        public FontFileLoader() { Debug.Assert(false); }

		public FontFileLoader(IFontSourceFactory fontSourceFactory)
		{
			_fontSourceFactory = fontSourceFactory;
		}

		[ComVisible(true)]
		public int CreateStreamFromKey(IntPtr fontFileReferenceKey,
									   uint fontFileReferenceKeySize,
									   out IDWriteFontFileStreamMirror fontFileStream)
		{
			uint numberOfCharacters = fontFileReferenceKeySize / 2;

			fontFileStream = null;
		   
			if ((fontFileReferenceKeySize % 2 != 0)                      // The fontFileReferenceKeySize must be divisible by sizeof(WCHAR)
				|| (numberOfCharacters <= 1)                                            // The fontFileReferenceKey cannot be less than or equal 1 character as it has to contain the NULL character.
				|| (Marshal.ReadInt16(fontFileReferenceKey, ((int)numberOfCharacters - 1) * 2) != '\0'))    // The fontFileReferenceKey must end with the NULL character
			{
				return unchecked((int)0x80070057); // E_INVALIDARG
			}

			string uriString = Marshal.PtrToStringUni(fontFileReferenceKey);

			int hr = 0;

			try
			{
				IFontSource fontSource = _fontSourceFactory.Create(uriString);        
				FontFileStream customFontFileStream = new FontFileStream(fontSource);

				fontFileStream = (IDWriteFontFileStreamMirror)customFontFileStream;
			}
			catch(System.Exception exception)
			{
				hr = Marshal.GetHRForException(exception);
			}

			return hr;
		}
	}
}
