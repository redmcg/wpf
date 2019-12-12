// main.c: Misc. functions for WPF

#include <windows.h>

HRESULT WINAPI MilVersionCheck(UINT uiCallerMilSdkVersion)
{
	if (uiCallerMilSdkVersion != 0x200184C0)
		return E_FAIL;
	return S_OK;
}
