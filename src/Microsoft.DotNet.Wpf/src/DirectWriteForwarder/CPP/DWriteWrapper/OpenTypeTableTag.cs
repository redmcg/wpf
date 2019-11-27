// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace MS.Internal.Text.TextInterface
{
    public enum OpenTypeTableTag
    {
		CharToIndexMap = 0x70616d63, /* 'cmap' */
		ControlValue = 0x20747663, /* 'cvt ' */
		BitmapData = 0x54444245, /* 'EBDT' */
		BitmapLocation = 0x434c4245, /* 'EBLC' */
		BitmapScale = 0x43534245, /* 'EBSC' */
		Editor0 = 0x30746465, /* 'edt0' */
		Editor1 = 0x31746465, /* 'edt1' */
		Encryption = 0x70797263, /* 'cryp' */
		FontHeader = 0x64616568, /* 'head' */
		FontProgram = 0x6d677066, /* 'fpgm' */
		GridfitAndScanProc = 0x70736167, /* 'gasp' */
		GlyphDirectory = 0x72696467, /* 'gdir' */
		GlyphData = 0x66796c67, /* 'glyf' */
		HoriDeviceMetrics = 0x786d6468, /* 'hdmx' */
		HoriHeader = 0x61656868, /* 'hhea' */
		HorizontalMetrics = 0x78746d68, /* 'hmtx' */
		IndexToLoc = 0x61636f6c, /* 'loca' */
		Kerning = 0x6e72656b, /* 'kern' */
		LinearThreshold = 0x4853544c, /* 'LTSH' */
		MaxProfile = 0x7078616d, /* 'maxp' */
		NamingTable = 0x656d616e, /* 'name' */
		OS_2 = 0x322f534f, /* 'OS/2' */
		Postscript = 0x74736f70, /* 'post' */
		PreProgram = 0x70657270, /* 'prep' */
		VertDeviceMetrics = 0x584d4456, /* 'VDMX' */
		VertHeader = 0x61656876, /* 'vhea' */
		VerticalMetrics = 0x78746d76, /* 'vmtx' */
		PCLT = 0x544c4350, /* 'PCLT' */
		TTO_GSUB = 0x42555347, /* 'GSUB' */
		TTO_GPOS = 0x534f5047, /* 'GPOS' */
		TTO_GDEF = 0x46454447, /* 'GDEF' */
		TTO_BASE = 0x45534142, /* 'BASE' */
		TTO_JSTF = 0x4654534a, /* 'JSTF' */
    }
}
