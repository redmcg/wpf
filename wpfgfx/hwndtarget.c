// Implementation of resource TYPE_HWNDRENDERTARGET

#include <windows.h>
#include "wpfgfx_private.h"

extern HRESULT HwndTarget_Command(MilChannel* channel, MilResourceHwndTarget* target,
	BYTE* data, UINT size)
{
	MILCMD_GENERIC *generic = (MILCMD_GENERIC*)data;

	switch (generic->Type)
	{
	case MilCmdHwndTargetCreate:
	{
		MILCMD_HWNDTARGET_CREATE *cmd = (MILCMD_HWNDTARGET_CREATE*)data;
		target->hwnd = (HWND)(ULONG_PTR)cmd->hwnd;
		return S_OK;
	}
	case MilCmdTargetInvalidate:
	{
		MILCMD_TARGET_INVALIDATE *cmd = (MILCMD_TARGET_INVALIDATE*)data;
		HDC hdc = GetDC(target->hwnd);
		HBRUSH brush;
		MilMessage present_msg;

		brush = CreateSolidBrush(RGB(255,0,255));

		FillRect(hdc, &cmd->rc, brush);

		DeleteObject(brush);

		ReleaseDC(target->hwnd, hdc);

		present_msg.Type = Presented;
		present_msg.Reserved = 0;
		present_msg.Data.Presented.PresentationResults = MIL_PRESENTATION_VSYNC_UNSUPPORTED;
		present_msg.Data.Presented.RefreshRate = 60;
		QueryPerformanceCounter(&present_msg.Data.Presented.PresentationTime);
		MilChannel_PostMessage(channel, &present_msg);
		return S_OK;
	}
	case MilCmdTargetSetClearColor:
		return S_OK;
	case MilCmdTargetSetRoot:
		return S_OK;
	case MilCmdTargetUpdateWindowSettings:
		return S_OK;
	default:
		return E_NOTIMPL;
	}
}

