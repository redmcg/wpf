// stubs.c: stub functions for WPF

#define COBJMACROS
#include <windows.h>
#include "wpfgfx_private.h"

HRESULT WINAPI MilVisualTarget_AttachToHwnd(HWND hwnd)
{
	return S_OK;
}

HRESULT WINAPI MilVisualTarget_DetachFromHwnd(HWND hwnd)
{
	return S_OK;
}

HRESULT WINAPI MilContent_AttachToHwnd(HWND hwnd)
{
	return S_OK;
}

HRESULT WINAPI MilContent_DetachFromHwnd(HWND hwnd)
{
	return S_OK;
}

BOOL WINAPI WgxConnection_ShouldForceSoftwareForGraphicsStreamClient(void)
{
	return FALSE;
}

LONG WINAPI MILAddRef(IUnknown *unk)
{
	return IUnknown_AddRef(unk);
}

LONG WINAPI MILRelease(IUnknown *unk)
{
	return IUnknown_Release(unk);
}

HRESULT WINAPI MILQueryInterface(IUnknown *unk, REFIID iid, void** obj)
{
	return IUnknown_QueryInterface(unk, iid, obj);
}
