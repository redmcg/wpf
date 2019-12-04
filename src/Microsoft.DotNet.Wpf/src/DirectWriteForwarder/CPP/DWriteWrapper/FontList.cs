// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;

namespace MS.Internal.Text.TextInterface
{
public class FontList : IEnumerable<Font>
{
	IDWriteFontList _fontList;

    public FontList(IDWriteFontList fontList)
    {
        _fontList = fontList;
    }
    
    public Font this[uint index]
    {
		get {
			IDWriteFont dwriteFont;
			dwriteFont = _fontList.GetFont(index);
			return new Font(dwriteFont);
		}
    }

    public uint Count
    {
		get {
			uint count = _fontList.GetFontCount();
			return count;
		}
    }
    
    public FontCollection FontsCollection
    {
		get {
			IDWriteFontCollection dwriteFontCollection;
			dwriteFontCollection = _fontList.GetFontCollection();
			return new FontCollection(dwriteFontCollection);
		}
    }

    protected IDWriteFontList FontListObject
    {
		get {
	        return _fontList;
		}
    }

	class FontsEnumerator : IEnumerator<Font>
	{
		FontList _fontList;
		long _currentIndex;

		internal FontsEnumerator(FontList fontList)
		{
			_fontList     = fontList;
			_currentIndex = -1;
		}

		public bool MoveNext()
		{
			if (_currentIndex >= _fontList.Count) //prevents _currentIndex from overflowing.
			{
				return false;
			}
			_currentIndex++;
			return _currentIndex < _fontList.Count;
		}

		public Font Current
		{
			get {
				if (_currentIndex >= _fontList.Count)
				{
					throw new InvalidOperationException(LocalizedErrorMsgs.EnumeratorReachedEnd);
				}
				else if (_currentIndex == -1)
				{
					throw new InvalidOperationException(LocalizedErrorMsgs.EnumeratorNotStarted);
				}
				return _fontList[(uint)_currentIndex];
			}
		}

		object IEnumerator.Current
		{
			get {
				return Current;
			}
		}

		public void Reset()
		{
			_currentIndex = -1;
		}

		public void Dispose()
		{
		}
	}

	IEnumerator<Font> IEnumerable<Font>.GetEnumerator()
	{
		return new FontsEnumerator(this);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new FontsEnumerator(this);
	}
}
}
