// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;

namespace MS.Internal.Text.TextInterface
{
public class LocalizedStrings : IDictionary<CultureInfo, string>
{
	IDWriteLocalizedStrings _localizedStrings;
	CultureInfo[] _keys;
	string[] _values;

    /// <summary>
    /// Constructs a LocalizedStrings object.
    /// </summary>
    /// <param name="localizedStrings">The DWrite localized Strings object that 
    /// this class wraps.</param>
    internal LocalizedStrings(IDWriteLocalizedStrings localizedStrings)
    {
        _localizedStrings = localizedStrings;
        _keys   = null;
        _values = null;
    }

    /// <summary>
    /// Constructs a LocalizedStrings object.
    /// </summary>
    /// <param name="localizedStrings">The DWrite localized Strings object that 
    /// this class wraps.</param>
    public LocalizedStrings()
    {
        _localizedStrings = null;
        _keys   = null;
        _values = null;
    }

    /// <summary>
    /// Gets the number of language/string pairs.
    /// </summary>
	public uint StringsCount
	{
		get {
			uint count = (_localizedStrings != null)? _localizedStrings.GetCount() : 0;
			return count;
		}
    }

	public int Count
	{
		get {
			uint count = StringsCount;

			if (count > (uint)Int32.MaxValue)
			{
				throw new OverflowException("The number of elements is greater than System.Int32.MaxValue");
			}

			return (int)count;
		}
    }

    /// <Summary>
    /// Lazily allocate the keys.
    /// </Summary>
	public ICollection<CultureInfo> Keys
    {
		get {
	        return (ICollection<CultureInfo>)KeysArray;
		}
    }

    /// <summary>
    /// Gets an array of the CultureInfos stored by the localizedStrings object.
    /// </summary>
    CultureInfo[] KeysArray
    {
		get {
			// Lazily allocate the keys.
			if (_keys == null)
			{
				_keys = new CultureInfo[StringsCount];
				for(uint i = 0; i < StringsCount; i++)
				{
					_keys[i] = new CultureInfo(GetLocaleName(i));
				}
			}
			return _keys;
		}
    }
    
    /// <Summary>
    /// Lazily allocate the values.
    /// </Summary>
    public ICollection<string> Values
    {
		get {
	        return (ICollection<String>)ValuesArray;
		}
    }

    /// <summary>
    /// Gets an array of the string values stored by the localizedStrings object.
    /// </summary>
    string[] ValuesArray
    {
		get {
			if (_values == null)
			{
				_values = new string[StringsCount];
				
				for(uint i = 0; i < StringsCount; i++)
				{
					_values[i] = GetString(i);
				}
			}

			return _values;
		}
	}

	public class LocalizedStringsEnumerator : IEnumerator<KeyValuePair<CultureInfo, string>>
	{
		LocalizedStrings _localizedStrings;
		long _currentIndex;

		internal LocalizedStringsEnumerator(LocalizedStrings localizedStrings)
		{
			_localizedStrings = localizedStrings;
			_currentIndex = 1;
		}

		public virtual bool MoveNext()
		{
			if (_currentIndex >= _localizedStrings.StringsCount) //prevents _currentIndex from overflowing.
			{
				return false;
			}
			_currentIndex++;
			return _currentIndex < _localizedStrings.StringsCount;
		}

		public virtual void Reset()
		{
			_currentIndex = -1;
		}
		
		public KeyValuePair<CultureInfo, string> Current
		{
			get {
				if (_currentIndex >= _localizedStrings.StringsCount)
				{
					throw new InvalidOperationException(LocalizedErrorMsgs.EnumeratorReachedEnd);
				}
				else if (_currentIndex == -1)
				{
					throw new InvalidOperationException(LocalizedErrorMsgs.EnumeratorNotStarted);
				}

				CultureInfo[] keys = _localizedStrings.KeysArray;
				string[] values = _localizedStrings.ValuesArray;
				KeyValuePair<CultureInfo, string> current = new KeyValuePair<CultureInfo, string>(keys[(uint)_currentIndex], values[(uint)_currentIndex]);
				return current;
			}
		}

		object IEnumerator.Current
		{
			get {
				return Current;
			}
		}

		void IDisposable.Dispose()
		{
		}
	}

    /// <summary>
    /// Gets the index of the item with the specified locale name.
    /// </summary>
    /// <param name="localeName">Locale name to look for.</param>
    /// <param name="index">Receives the zero-based index of the locale name/string pair.</param>
    /// <returns>TRUE if the locale name exists or FALSE if not.</returns>
    public bool FindLocaleName(
        string localeName,
        [Out] out uint index)
    {
        if (_localizedStrings == null)
        {
            index = 0;
            return false;
        }
        else
        {
            bool exists = false;
            uint localeNameIndex = 0;
            _localizedStrings.FindLocaleName(
											 localeName,
											 out localeNameIndex,
											 out exists
											 );
            index = localeNameIndex;
            return (!!exists);
        }
    }

    /// <summary>
    /// Gets the length in characters (not including the null terminator) of the locale name with the specified index.
    /// </summary>
    /// <param name="index">Zero-based index of the locale name.</param>
    /// <returns>The length in characters, not including the null terminator.</returns>
    uint GetLocaleNameLength(uint index)
    {
        if (_localizedStrings == null)
        {
            return 0;
        }
        else
        {
            return _localizedStrings.GetLocaleNameLength(index);
        }
    }

    /// <summary>
    /// Gets the locale name with the specified index.
    /// </summary>
    /// <param name="index">Zero-based index of the locale name.</param>
    /// <returns>The locale name.</returns>
    internal string GetLocaleName(uint index)
    {        
        if (_localizedStrings == null)
        {
            return String.Empty;
        }
        else
        {
            uint localeNameLength = this.GetLocaleNameLength(index);
            Debug.Assert(localeNameLength >= 0 && localeNameLength < UInt32.MaxValue);
            IntPtr localeNameWCHAR = Marshal.AllocCoTaskMem(2 * ((int)localeNameLength + 1));
            try
            {
                _localizedStrings.GetLocaleName(index, localeNameWCHAR, localeNameLength + 1);
                string localeName = Marshal.PtrToStringUni(localeNameWCHAR);
                return localeName;
            }
            finally
            {
				Marshal.FreeCoTaskMem(localeNameWCHAR);
            }
        }
    }

    /// <summary>
    /// Gets the length in characters (not including the null terminator) of the string with the specified index.
    /// </summary>
    /// <param name="index">Zero-based index of the string.</param>
    /// <returns>The length in characters, not including the null terminator.</returns>
    uint GetStringLength(uint index)
    {
        if (_localizedStrings == null)
        {
            return 0;
        }
        else
        {
            uint length = 0;
            length = _localizedStrings.GetStringLength(index);
            return length;
        }
    }

    /// <summary>
    /// Gets the string with the specified index.
    /// </summary>
    /// <param name="index">Zero-based index of the string.</param>
    /// <returns>The string.</returns>
    public string GetString(uint index)
    {        
        if (_localizedStrings == null)
        {
            return String.Empty;
        }
        else
        {
            uint stringLength = this.GetStringLength(index);
            Debug.Assert(stringLength >= 0 && stringLength < UInt32.MaxValue);
            IntPtr stringWCHAR = Marshal.AllocCoTaskMem(2 * ((int)stringLength + 1));
            
            try
            {
                _localizedStrings.GetString(index, stringWCHAR, stringLength + 1);
				return Marshal.PtrToStringUni(stringWCHAR);
            }
            finally
            {
				Marshal.FreeCoTaskMem(stringWCHAR);
            }
        }
    }

	public virtual void Add(CultureInfo key, string value)
	{
		throw new NotSupportedException();
	}

	public virtual bool ContainsKey(CultureInfo key)
	{
		uint index = 0;
		return FindLocaleName(key.Name, out index);
	}

	public virtual bool Remove(CultureInfo key)
	{
		throw new NotSupportedException();
	}

	public virtual bool TryGetValue(CultureInfo key, [Out] out string value)
	{
		Debug.Assert(key != null);

		uint index = 0;
		value = String.Empty;
		if (FindLocaleName(key.Name, out index))
		{
			value = GetString(index);
			return true;
		}
		return false;                
	}

	public string this[CultureInfo key]
	{
		get {
			string value;
			if (TryGetValue(key, out value))
			{
				return value;
			}
			else
			{
				return null;
			}
		}
		set {
			throw new NotSupportedException();
		}
	}

	public virtual void Add(KeyValuePair<CultureInfo, string> item)
	{
		throw new NotSupportedException();
	}

	public virtual void Clear()
	{
		throw new NotSupportedException();
	}

	public virtual bool Contains(KeyValuePair<CultureInfo, string> item)
	{
		throw new NotSupportedException();
	}

	public virtual void CopyTo(KeyValuePair<CultureInfo, string>[] arrayObj, int arrayIndex)
	{
		int index = arrayIndex;
		foreach (KeyValuePair<CultureInfo, string> pair in this)
		{
			arrayObj[index] = pair;
			++index;
		}
	}

	public bool IsReadOnly
	{
		get { return true; }
	}

	public virtual bool Remove(KeyValuePair<CultureInfo, string> item)
	{
		throw new NotSupportedException();
	}

	public virtual IEnumerator<KeyValuePair<CultureInfo, string>> GetEnumerator()
	{
		return new LocalizedStringsEnumerator(this);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
}
