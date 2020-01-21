// main.c: Misc. functions for WPF

#include <windows.h>
#include "wine/debug.h"

WINE_DEFAULT_DEBUG_CHANNEL(wpfgfx);

HRESULT WINAPI MilVersionCheck(UINT uiCallerMilSdkVersion)
{
	WINE_TRACE("%x\n", uiCallerMilSdkVersion);
	if (uiCallerMilSdkVersion != 0x200184C0)
		return E_FAIL;
	return S_OK;
}
