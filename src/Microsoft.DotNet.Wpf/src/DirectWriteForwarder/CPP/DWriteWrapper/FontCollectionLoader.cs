// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MS.Internal.Text.TextInterface
{
    [ClassInterface(ClassInterfaceType.None), ComVisible(true)]
    public class FontCollectionLoader : IDWriteFontCollectionLoaderMirror
    {
		IFontSourceCollectionFactory _fontSourceCollectionFactory;
		IntPtr _fontFileLoader;

		public FontCollectionLoader() { Debug.Assert(false); }
		
		public FontCollectionLoader(IFontSourceCollectionFactory fontSourceCollectionFactory,
								    IntPtr fontFileLoader)
		{
			_fontSourceCollectionFactory = fontSourceCollectionFactory;
			_fontFileLoader              = fontFileLoader;
		}

		[ComVisible(true)]    
		public int CreateEnumeratorFromKey(IDWriteFactory factory,
										   IntPtr collectionKey,
										   uint collectionKeySize,
										   out IDWriteFontFileEnumeratorMirror fontFileEnumerator)
		{
			uint numberOfCharacters = collectionKeySize / 2;
			fontFileEnumerator = null;
			if (   (collectionKeySize % 2 != 0)                        // The collectionKeySize must be divisible by sizeof(WCHAR)
				|| (numberOfCharacters <= 1)                                       // The collectionKey cannot be less than or equal 1 character as it has to contain the NULL character.
				|| (Marshal.ReadInt16(collectionKey, ((int)numberOfCharacters - 1) * 2) != '\0'))  // The collectionKey must end with the NULL character
			{
				return unchecked((int)0x80070057); // E_INVALIDARG
			}

			fontFileEnumerator = null;

			string uriString = Marshal.PtrToStringUni(collectionKey);
			int hr = 0;

			try
			{
				IFontSourceCollection fontSourceCollection = _fontSourceCollectionFactory.Create(uriString);
				FontFileEnumerator fontFileEnum = new FontFileEnumerator(
													  fontSourceCollection,
													  _fontFileLoader,
													  factory
													  );
				fontFileEnumerator = (IDWriteFontFileEnumeratorMirror)fontFileEnum;
			}
			catch(Exception exception)
			{
				hr = Marshal.GetHRForException(exception);
			}

			return hr;
		}
	}
}
