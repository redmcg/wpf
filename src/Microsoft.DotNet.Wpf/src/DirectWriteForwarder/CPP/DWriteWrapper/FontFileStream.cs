// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace MS.Internal.Text.TextInterface
{
[ClassInterface(ClassInterfaceType.None)]
[ComVisible(true)]
public class FontFileStream : IDWriteFontFileStreamMirror
{
	Stream _fontSourceStream;
	long _lastWriteTime;
	object _fontSourceStreamLock;

    public FontFileStream(IFontSource fontSource)
    {
        // Previously we were using fontSource->GetStream() which caused crashes in the XPS scenarios
        // as the stream was getting closed by some other object. In XPS scenarios GetStream() would return
        // MS::Internal::IO:Packaging::SynchronizingStream which is owned by the XPS docs and
        // is known to have issues regarding the lifetime management where by if the current XPS page is 
        // flipped then the stream will get disposed. Thus, we cannot rely on the stream directly and hence we now use
        // fontSource->GetUnmanagedStream() which returns a copy of the content of the stream. Special casing XPS will not
        // guarantee that this problem will be fixed so we will use the GetUnmanagedStream(). Note: This path will only 
        // be taken for embedded fonts among which XPS is a main scenario. For local fonts we use DWrite's APIs.
        _fontSourceStream = fontSource.GetUnmanagedStream();
        try
        {
            _lastWriteTime = fontSource.GetLastWriteTimeUtc().ToFileTimeUtc();
        }    
        catch (ArgumentOutOfRangeException) //The resulting file time would represent a date and time before 12:00 midnight January 1, 1601 C.E. UTC. 
        {
            _lastWriteTime = -1;
        }

        // Create lock to control access to font source stream.
        _fontSourceStreamLock = new Object();
    }

    ~FontFileStream()
    {
        _fontSourceStream.Close();
    }

    [ComVisible(true)]
    public int ReadFileFragment(
        out IntPtr fragmentStart,
        ulong fileOffset,
     	ulong fragmentSize,
        out IntPtr fragmentContext
        )
    {
        int hr = 0;
        try
        {
            if(
                fileOffset   > Int64.MaxValue                    // Cannot safely cast to long
                ||            
                fragmentSize > Int32.MaxValue                     // fragment size cannot be casted to int
                || 
                fileOffset > UInt64.MaxValue - fragmentSize           // make sure next sum doesn't overflow
                || 
                fileOffset + fragmentSize  > (ulong)_fontSourceStream.Length // reading past the end of the Stream
              ) 
            {
				fragmentStart = IntPtr.Zero;
				fragmentContext = IntPtr.Zero;
                return unchecked((int)0x80070057); // E_INVALIDARG
            }

            int fragmentSizeInt = (int)fragmentSize;
            byte[] buffer = new byte[fragmentSizeInt];
            
            // DWrite may call this method from multiple threads. We need to ensure thread safety by making Seek and Read atomic.
            Monitor.Enter(_fontSourceStreamLock);
            try
            {
                _fontSourceStream.Seek((long)fileOffset,
                                       SeekOrigin.Begin);

                _fontSourceStream.Read(buffer,         //byte[]
                                       0,              //int
                                       fragmentSizeInt //int
                                       );
            }
            finally 
            {
                Monitor.Exit(_fontSourceStreamLock);
            }

            GCHandle gcHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            fragmentStart = gcHandle.AddrOfPinnedObject();
            
            fragmentContext = GCHandle.ToIntPtr(gcHandle);
        }
        catch(Exception exception)
        {
			fragmentStart = IntPtr.Zero;
			fragmentContext = IntPtr.Zero;
            hr = Marshal.GetHRForException(exception);
        }

        return hr;
    }

    [ComVisible(true)]
    public void ReleaseFileFragment(
        IntPtr fragmentContext
        )
    {
        if (fragmentContext != IntPtr.Zero)
        {
            GCHandle gcHandle = GCHandle.FromIntPtr(fragmentContext);
            gcHandle.Free();
        }
    }

    [ComVisible(true)]
    public int GetFileSize(
        out ulong fileSize
        )
    {
        int hr = 0;
        try
        {
            fileSize = (ulong)_fontSourceStream.Length;
        }
        catch(Exception exception)
        {
			fileSize = 0;
            hr = Marshal.GetHRForException(exception);
        }

        return hr;
     }

    public int GetLastWriteTime(
        out ulong lastWriteTime
        )
    {
        if (_lastWriteTime < 0) //The resulting file time would represent a date and time before 12:00 midnight January 1, 1601 C.E. UTC.
        {
			lastWriteTime = 0;
            return unchecked((int)0x80004005); // E_FAIL
        }
        lastWriteTime = (ulong)_lastWriteTime;
        return 0;
    }
}
}
