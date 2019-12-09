// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MS.Internal.Text.TextInterface
{
    [ClassInterface(ClassInterfaceType.None), ComVisible(true)]
    public class FontFileEnumerator : IDWriteFontFileEnumeratorMirror
    {     
		IEnumerator<IFontSource> _fontSourceCollectionEnumerator;
		FontFileLoader _fontFileLoader;
		IDWriteFactory _factory;

		public FontFileEnumerator() { Debug.Assert(false); }

		public FontFileEnumerator(IEnumerable<IFontSource> fontSourceCollection,
								  FontFileLoader fontFileLoader,
								  IDWriteFactory factory)
		{
			_fontSourceCollectionEnumerator = fontSourceCollection.GetEnumerator();
			_fontFileLoader                 = fontFileLoader;
			_factory                        = factory;
		}

		[ComVisible(true)]
		public int MoveNext(out bool hasCurrentFile)
		{
			int hr = 0;
			hasCurrentFile = false;
			try
			{
				hasCurrentFile = _fontSourceCollectionEnumerator.MoveNext();
			}
			catch(System.Exception exception)
			{
				hr = Marshal.GetHRForException(exception);
			}

			return hr;
		}

		public int GetCurrentFontFile(out IDWriteFontFile fontFile) 
		{
			fontFile = null;
			try {
				fontFile = Factory.CreateFontFile(
											  _factory,
											  _fontFileLoader,
											  _fontSourceCollectionEnumerator.Current.Uri
											  );
			}
			catch (Exception e)
			{
				return Marshal.GetHRForException(e);
			}
			return 0;
		}
	}
}
