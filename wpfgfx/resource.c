// channel.c - Resource handle functions

#include <windows.h>
#include <assert.h>
#include "wpfgfx_private.h"
#include "wine/debug.h"

WINE_DEFAULT_DEBUG_CHANNEL(wpfgfx);

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
	channel->resource_refcounts[handle] = 1;
}

HRESULT lookup_resource_handle(MilChannel* channel, ResourceHandle handle, MilResource** result, LONG** refcount)
{
	if (handle >= ARRAY_SIZE(channel->resources))
		return E_HANDLE;

	if (!channel->resources[handle])
		return E_HANDLE;

	*result = channel->resources[handle];
	if (refcount)
		*refcount = &channel->resource_refcounts[handle];

	return S_OK;
}

static void clear_resource_handle(MilChannel* channel, ResourceHandle handle)
{
	assert(handle < ARRAY_SIZE(channel->resources));
	assert(channel->resources[handle]);

	channel->resources[handle] = NULL;
	channel->resource_refcounts[handle] = 0;

	if (channel->first_free_resource > handle)
		channel->first_free_resource = handle;
}

HRESULT WINAPI MilResource_ReleaseOnChannel(MilChannel* channel, ResourceHandle hResource, BOOL* deleted)
{
	HRESULT hr;
	MilResource* object;
	LONG* refcount;

	WINE_TRACE("%p,%u,%p\n", channel, hResource, deleted);

	if (!channel || !deleted)
		return E_POINTER;
	
	hr = lookup_resource_handle(channel, hResource, &object, &refcount);
	if (FAILED(hr))
		return hr;

	if (InterlockedDecrement(refcount) == 0)
	{
		*deleted = TRUE;
		clear_resource_handle(channel, hResource);

		if (InterlockedDecrement(&object->RefCount) == 0)
		{
			free(object);
		}
	}
	else
		*deleted = FALSE;
	
	return S_OK;
}

void destroy_channel_resources(MilChannel *channel)
{
	int i;
	BOOL deleted;
	for (i=1; i < ARRAY_SIZE(channel->resource_refcounts); i++)
	{
		while (channel->resource_refcounts[i])
			MilResource_ReleaseOnChannel(channel, i, &deleted);
	}
}

HRESULT WINAPI MilResource_CreateOrAddRefOnChannel(MilChannel* channel, ResourceType restype,  ResourceHandle* handle)
{
	HRESULT hr;
	ResourceHandle result_handle;
	MilResource* new_resource = NULL;

	WINE_TRACE("%p,%u,%p->%u\n", channel, restype, handle, handle ? *handle : 0);

	if (!channel || !handle)
		return E_POINTER;

	if (*handle != 0)
	{
		// AddRef
		MilResource *resource;
		LONG *refcount;

		hr = lookup_resource_handle(channel, *handle, &resource, &refcount);
		if (SUCCEEDED(hr))
		{
			if (restype != resource->Type)
			{
				WINE_WARN("expected %i type resource but handle was type %i\n",
					restype, resource->Type);
				hr = E_INVALIDARG;
			}
		}
		if (SUCCEEDED(hr))
			InterlockedIncrement(refcount);
		return hr;
	}

	hr = find_free_handle(channel, &result_handle);
	if (FAILED(hr))
		return hr;

	switch (restype)
	{
	case TYPE_ETWEVENTRESOURCE:
	case TYPE_VISUAL:
	case TYPE_MATRIXTRANSFORM:
	case TYPE_RENDERDATA:
		new_resource = malloc(sizeof(MilResource));
		if (!new_resource)
			return E_OUTOFMEMORY;
		new_resource->Type = restype;
		new_resource->RefCount = 1;
		break;
	case TYPE_HWNDRENDERTARGET:
	{
		MilResourceHwndTarget *obj = malloc(sizeof(MilResourceHwndTarget));
		new_resource = &obj->resource;
		if (!new_resource)
			return E_OUTOFMEMORY;
		new_resource->Type = restype;
		new_resource->RefCount = 1;
		obj->hwnd = NULL;
		break;
	}
	case TYPE_SOLIDCOLORBRUSH:
	{
		MilResourceSolidColorBrush *obj = malloc(sizeof(MilResourceSolidColorBrush));
		new_resource = &obj->resource;
		if (!new_resource)
			return E_OUTOFMEMORY;
		new_resource->Type = restype;
		new_resource->RefCount = 1;
		memset(&obj->color, 0, sizeof(obj->color));
		break;
	}
	default:
		WINE_FIXME("Unimplemented resource type %i\n", restype);
		return E_NOTIMPL;
	}

	set_resource_handle(channel, result_handle, new_resource);

	*handle = result_handle;

	WINE_TRACE("<-- %i\n", result_handle);

	return S_OK;
}

HRESULT WINAPI MilResource_DuplicateHandle(MilChannel* srcchan, ResourceHandle src, MilChannel* dstchan, ResourceHandle* dst)
{
	HRESULT hr;
	MilResource* object;
	ResourceHandle dst_handle;

	WINE_TRACE("%p,%u,%p,%p\n", srcchan, src, dstchan, dst);

	if (!srcchan || !dstchan || !dst)
		return E_POINTER;
	
	hr = find_free_handle(dstchan, &dst_handle);
	if (FAILED(hr))
		return hr;

	hr = lookup_resource_handle(srcchan, src, &object, NULL);
	if (FAILED(hr))
		return hr;

	assert(InterlockedIncrement(&object->RefCount) >= 2);

	set_resource_handle(dstchan, dst_handle, object);
	*dst = dst_handle;

	return S_OK;
}
