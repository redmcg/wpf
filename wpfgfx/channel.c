// channel.c - Connection/Channel functions

#include <windows.h>
#include "wpfgfx_private.h"

HRESULT WINAPI WgxConnection_Create(BOOL requestSynchronousTransport,
	void** ppConnection)
{
	if (!ppConnection)
		return E_POINTER;
	*ppConnection = (void*)0xdeadbeef;
	return S_OK;
}

HRESULT WINAPI WgxConnection_Disconnect(void* pTranspManager)
{
	return S_OK;
}

HRESULT WINAPI MilConnection_CreateChannel(void* pTransport, MilChannel* referenceChannel, MilChannel** result)
{
	if (!pTransport || !result)
		return E_POINTER;
	
	*result = malloc(sizeof(**result));
	if (!*result)
		return E_OUTOFMEMORY;

	(*result)->transport = pTransport;
	(*result)->notify_hwnd = 0;
	(*result)->notify_msg = 0;
	memset((*result)->resources, 0, sizeof((*result)->resources));
	memset((*result)->resource_refcounts, 0, sizeof((*result)->resource_refcounts));
	(*result)->first_free_resource = 1;
	return S_OK;
}

#define VALIDATE_STRUCT(t, st) case t: if (size < sizeof(st)) return E_INVALIDARG; break

static HRESULT validate_command(BYTE* data, UINT size)
{
	if (!data)
		return E_POINTER;
	
	if (size < 4)
		return E_INVALIDARG;

	switch (*(MILCMD*)data)
	{
	VALIDATE_STRUCT(MilCmdPartitionRegisterForNotifications,
		MILCMD_PARTITION_REGISTERFORNOTIFICATIONS);
	VALIDATE_STRUCT(MilCmdChannelRequestTier,
		MILCMD_PARTITION_REQUESTTIER);
	VALIDATE_STRUCT(MilCmdHwndTargetCreate,
		MILCMD_HWNDTARGET_CREATE);
	VALIDATE_STRUCT(MilCmdPartitionNotifyPolicyChangeForNonInteractiveMode,
		MILCMD_PARTITION_NOTIFYPOLICYCHANGEFORNONINTERACTIVEMODE);
	default:
		return E_NOTIMPL;
	}

	return S_OK;
}

HRESULT WINAPI MilResource_SendCommand(BYTE* data, UINT size, BOOL sendInSeparateBatch, MilChannel* channel)
{
	HRESULT hr;

	if (!channel)
		return E_POINTER;

	hr = validate_command(data, size);
	if (FAILED(hr))
		return hr;

	/* TODO: Save command to batch */

	return S_OK;
}

HRESULT WINAPI MilChannel_SetNotificationWindow(MilChannel* channel, HWND hwnd, UINT msg)
{
	if (!channel)
		return E_POINTER;
	
	channel->notify_hwnd = hwnd;
	channel->notify_msg = msg;

	return S_OK;
}

HRESULT WINAPI MilChannel_CloseBatch(MilChannel* channel)
{
	if (!channel)
		return E_POINTER;
	
	return S_OK;
}

HRESULT WINAPI MilChannel_CommitChannel(MilChannel* channel)
{
	if (!channel)
		return E_POINTER;
	
	return S_OK;
}

HRESULT WINAPI MilComposition_SyncFlush(MilChannel* channel)
{
	if (!channel)
		return E_POINTER;
	
	return S_OK;
}

HRESULT WINAPI MilComposition_PeekNextMessage(MilChannel* channel, MilMessage* message, INT_PTR size, BOOL* messageRetrieved)
{
	if (!channel || !message || !messageRetrieved)
		return E_POINTER;
	
	*messageRetrieved = FALSE;

	return S_OK;
}
