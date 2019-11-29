// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices;

namespace MS.Internal.Text.TextInterface
{
    /// <summary>
    /// This interface is used as a level on indirection for classes in managed c++ to be able to utilize methods
    /// from the static class Classification present in PresentationCore.dll.
    /// We cannot make MC++ reference PresentationCore.dll since this will result in cirular reference.
    /// </summary>
    public interface IClassification
    {
        void GetCharAttribute(
            int unicodeScalar,
            [Out] out bool isCombining,
            [Out] out bool needsCaretInfo,
            [Out] out bool isIndic,
            [Out] out bool isDigit,
            [Out] out bool isLatin,
            [Out] out bool isStrong
            );
    }
}
