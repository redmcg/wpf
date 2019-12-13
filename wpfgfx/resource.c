// channel.c - Resource handle functions

#include <windows.h>
#include <assert.h>
#include "wpfgfx_private.h"

static HRESULT find_free_handle(MilChannel* channel, ResourceHandle *handle)
{
	while (channel->first_free_resource < ARRAY_SIZE(channel->resources))
	{
		if (!channel->resources[channel->first_free_resource])
		{
			*handle = channel->first_free_resource;
			return S_OK;
		}

		channel->first_free_resource++;
	}

	return E_OUTOFMEMORY;
}

static void set_resource_handle(MilChannel* channel, ResourceHandle handle, MilResource* resource)
{
	assert(handle < ARRAY_SIZE(channel->resources));
	assert(!channel->resources[handle]);

	channel->resources[handle] = resource;
}

HRESULT WINAPI MilResource_CreateOrAddRefOnChannel(MilChannel* channel, ResourceType restype,  ResourceHandle* handle)
{
	HRESULT hr;
	ResourceHandle result_handle;
	MilResource* new_resource = NULL;

	if (!channel || !handle)
		return E_POINTER;
	
	hr = find_free_handle(channel, &result_handle);
	if (FAILED(hr))
		return hr;

	switch (restype)
	{
	case TYPE_ETWEVENTRESOURCE:
	case TYPE_VISUAL:
	case TYPE_HWNDRENDERTARGET:
		new_resource = malloc(sizeof(MilResource));
		if (!new_resource)
			return E_OUTOFMEMORY;
		new_resource->Type = restype;
		break;
	default:
		return E_NOTIMPL;
	}

	set_resource_handle(channel, result_handle, new_resource);

	*handle = result_handle;

	return S_OK;
}

HRESULT WINAPI MilResource_DuplicateHandle(MilChannel* srcchan, ResourceHandle src, MilChannel* dstchan, ResourceHandle* dst)
{
	if (!srcchan || !dstchan || !dst)
		return E_POINTER;

	return E_NOTIMPL;
}
