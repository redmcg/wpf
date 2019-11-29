// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices;

namespace MS.Internal.Text.TextInterface
{
    /// <summary>
    /// The DWRITE_MATRIX structure specifies the graphics transform to be applied
    /// to rendered glyphs.
    /// </summary>
	[StructLayout (LayoutKind.Sequential)]
    public struct DWriteMatrix
    {
        /// <summary>
        /// Horizontal scaling / cosine of rotation
        /// </summary>
        public float M11;

        /// <summary>
        /// Horizontal shear / sine of rotation
        /// </summary>
    	public float M12;

        /// <summary>
        /// Vertical shear / negative sine of rotation
        /// </summary>
        public float M21;

        /// <summary>
        /// Vertical scaling / cosine of rotation
        /// </summary>
        public float M22;

        /// <summary>
        /// Horizontal shift (always orthogonal regardless of rotation)
        /// </summary>
        public float Dx;

        /// <summary>
        /// Vertical shift (always orthogonal regardless of rotation)
        /// </summary>
        public float Dy;
    }
}
