// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//---------------------------------------------------------------------------
//

// 
// Description: The FontSourceCollection class represents a collection of font files.
//
//  
//
//
//---------------------------------------------------------------------------

using System;
using System.IO;


namespace MS.Internal.Text.TextInterface
{
    public interface IFontSource
    {
        void TestFileOpenable();
        UnmanagedMemoryStream GetUnmanagedStream();
        DateTime GetLastWriteTimeUtc();
        Uri Uri
        {
            get;
        }

        bool IsComposite
        {
            get;
        }
       
    }

    public interface IFontSourceFactory
    {
        IFontSource Create(string uriString);
    }
}
