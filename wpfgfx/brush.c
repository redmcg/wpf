// Implementation of brush type resources

#include <windows.h>
#include "wpfgfx_private.h"
#include "wine/debug.h"

WINE_DEFAULT_DEBUG_CHANNEL(wpfgfx);

extern HRESULT SolidColorBrush_Command(MilChannel* channel, MilResourceSolidColorBrush* brush,
	BYTE* data, UINT size)
{
	MILCMD_GENERIC *generic = (MILCMD_GENERIC*)data;

	switch (generic->Type)
	{
	case MilCmdSolidColorBrush:
	{
		MILCMD_SOLIDCOLORBRUSH *cmd = (MILCMD_SOLIDCOLORBRUSH*)data;
		WINE_TRACE("MilCmdSolidColorBrush handle=%i\n", cmd->Handle);
		brush->color = cmd->Color;
		return S_OK;
	}
	default:
		WINE_FIXME("Unimplemented cmd %i\n", generic->Type);
		return E_NOTIMPL;
	}
}

