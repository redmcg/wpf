// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace MS.Internal.Text.TextInterface
{
    /// <summary>
    /// Optional adjustment to a glyph's position. An glyph offset changes the position of a glyph without affecting
    /// the pen position. Offsets are in logical, pre-transform units.
    /// </summary>
    public struct DWriteGlyphOffset
    {       
        /// <summary>
        /// Offset in the advance direction of the run. A positive advance offset moves the glyph to the right
        /// (in pre-transform coordinates) if the run is left-to-right or to the left if the run is right-to-left.
        /// </summary>
        public float AdvanceOffset;

        /// <summary>
        /// Offset in the ascent direction, i.e., the direction ascenders point. A positive ascender offset moves
        /// the glyph up (in pre-transform coordinates).
        /// </summary>
        public float AscenderOffset;
    }
}
