// channel.c - Connection/Channel functions

#include <windows.h>
#include "wpfgfx_private.h"
#include "wine/debug.h"

WINE_DEFAULT_DEBUG_CHANNEL(wpfgfx);

HRESULT WINAPI WgxConnection_Create(BOOL requestSynchronousTransport,
	void** ppConnection)
{
	WINE_TRACE("%i,%p\n", requestSynchronousTransport, ppConnection);
	if (!ppConnection)
		return E_POINTER;
	*ppConnection = (void*)0xdeadbeef;
	return S_OK;
}

HRESULT WINAPI WgxConnection_Disconnect(void* pTranspManager)
{
	WINE_FIXME("%p\n", pTranspManager);
	return S_OK;
}

HRESULT WINAPI MilConnection_CreateChannel(void* pTransport, MilChannel* referenceChannel, MilChannel** result)
{
	WINE_TRACE("%p,%p,%p\n", pTransport, referenceChannel, result);

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
	(*result)->message_queue = NULL;
	(*result)->last_message = &(*result)->message_queue;
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
	VALIDATE_STRUCT(MilCmdMatrixTransform,
		MILCMD_MATRIXTRANSFORM);
	VALIDATE_STRUCT(MilCmdTargetInvalidate,
		MILCMD_TARGET_INVALIDATE);
	VALIDATE_STRUCT(MilCmdTargetSetClearColor,
		MILCMD_TARGET_SETCLEARCOLOR);
	VALIDATE_STRUCT(MilCmdTargetSetRoot,
		MILCMD_TARGET_SETROOT);
	VALIDATE_STRUCT(MilCmdTargetUpdateWindowSettings,
		MILCMD_TARGET_UPDATEWINDOWSETTINGS);
	VALIDATE_STRUCT(MilCmdVisualInsertChildAt,
		MILCMD_VISUAL_INSERTCHILDAT);
	VALIDATE_STRUCT(MilCmdVisualRemoveAllChildren,
		MILCMD_VISUAL_REMOVEALLCHILDREN);
	VALIDATE_STRUCT(MilCmdVisualSetTransform,
		MILCMD_VISUAL_SETTRANSFORM);
	VALIDATE_STRUCT(MilCmdPartitionNotifyPolicyChangeForNonInteractiveMode,
		MILCMD_PARTITION_NOTIFYPOLICYCHANGEFORNONINTERACTIVEMODE);
	default:
		WINE_FIXME("Unimplemented cmd 0x%x\n", *(MILCMD*)data);
		return E_NOTIMPL;
	}

	return S_OK;
}

HRESULT MilChannel_dispatch_command(MilChannel* channel, BYTE* data, UINT size)
{
	HRESULT hr;
	MILCMD_GENERIC *cmd = (MILCMD_GENERIC*)data;

	switch (cmd->Type)
	{
	/* Commands that apply to a resource */
	case MilCmdHwndTargetCreate:
	case MilCmdMatrixTransform:
	case MilCmdTargetInvalidate:
	case MilCmdTargetSetClearColor:
	case MilCmdTargetSetRoot:
	case MilCmdTargetUpdateWindowSettings:
	case MilCmdVisualInsertChildAt:
	case MilCmdVisualRemoveAllChildren:
	case MilCmdVisualSetTransform:
	{
		MilResource *resource;

		hr = lookup_resource_handle(channel, cmd->Handle, &resource, NULL);
		if (FAILED(hr))
			return hr;

		switch (resource->Type)
		{
		case TYPE_HWNDRENDERTARGET:
			return HwndTarget_Command(channel, (MilResourceHwndTarget*)resource,
				data, size);
		default:
			return S_OK;
		}
	}
	/* Commands that apply to the partition */
	case MilCmdChannelRequestTier:
	{
		MilMessage reply;
		reply.Type = Caps;
		reply.Reserved = 0;
		memset(&reply.Data.Caps, 0, sizeof(reply.Data.Caps));
		MilChannel_PostMessage(channel, &reply);
		return S_OK;
	}
	case MilCmdPartitionRegisterForNotifications:
	case MilCmdPartitionNotifyPolicyChangeForNonInteractiveMode:
	default:
		return S_OK;
	}
}

HRESULT WINAPI MilResource_SendCommand(BYTE* data, UINT size, BOOL sendInSeparateBatch, MilChannel* channel)
{
	HRESULT hr;

	WINE_TRACE("%p,%u,%i,%p: cmd=%i\n", data, size, sendInSeparateBatch, channel,
		size >= 4 ? *(INT*)data: -1);

	if (!channel)
		return E_POINTER;

	hr = validate_command(data, size);
	if (FAILED(hr))
		return hr;

	hr = MilChannel_dispatch_command(channel, data, size);

	return hr;
}

HRESULT WINAPI MilChannel_SetNotificationWindow(MilChannel* channel, HWND hwnd, UINT msg)
{
	WINE_TRACE("%p,%p,%u\n", channel, hwnd, msg);

	if (!channel)
		return E_POINTER;
	
	channel->notify_hwnd = hwnd;
	channel->notify_msg = msg;

	return S_OK;
}

HRESULT WINAPI MilChannel_CloseBatch(MilChannel* channel)
{
	WINE_TRACE("%p\n", channel);

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
	WINE_TRACE("%p\n", channel);

	MilMessage msg;

	if (!channel)
		return E_POINTER;

	msg.Type = SyncFlushReply;
	msg.Reserved = 0;
	MilChannel_PostMessage(channel, &msg);

	return S_OK;
}

HRESULT MilChannel_PostMessage(MilChannel* channel, const MilMessage* msg)
{
	MilMessageLink* link;

	WINE_TRACE("%p,%p->%i\n", channel, msg, (msg != NULL) ? msg->Type : 0);

	if (!channel || !msg)
		return E_POINTER;

	link = malloc(sizeof(*link));

	if (!link)
		return E_OUTOFMEMORY;

	link->msg = *msg;
	link->next = NULL;
	*(channel->last_message) = link;
	channel->last_message = &link->next;
	if (!channel->message_queue)
		channel->message_queue = link;

	if (channel->notify_hwnd)
		PostMessageW(channel->notify_hwnd, channel->notify_msg, 0, 0);

	return S_OK;
}

HRESULT WINAPI MilComposition_PeekNextMessage(MilChannel* channel, MilMessage* message, INT_PTR size, BOOL* messageRetrieved)
{
	WINE_TRACE("%p\n", channel);

	if (!channel || !message || !messageRetrieved)
		return E_POINTER;

	if (size < sizeof(MilMessage))
		return E_INVALIDARG;

	if (channel->message_queue)
	{
		*message = channel->message_queue->msg;
		*messageRetrieved = TRUE;
		if (channel->message_queue->next)
		{
			MilMessageLink* tmp = channel->message_queue;
			channel->message_queue = channel->message_queue->next;
			free(tmp);
		}
		else
		{
			free(channel->message_queue);
			channel->message_queue = NULL;
			channel->last_message = &channel->message_queue;
		}
	}
	else
	{
		*messageRetrieved = FALSE;
	}

	WINE_TRACE("<-- %i,%i\n", *messageRetrieved, *message);

	return S_OK;
}
