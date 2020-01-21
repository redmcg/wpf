// factory.c: MILFactory2 implementation

#define COBJMACROS
#include <windows.h>
#include "wpfgfx_private.h"
#include "wine/debug.h"

WINE_DEFAULT_DEBUG_CHANNEL(wpfgfx);

static inline MILFactory2 *impl_from_IUnknown(IUnknown *iface)
{
	return CONTAINING_RECORD(iface, MILFactory2, IUnknown_iface);
}

static HRESULT WINAPI MILFactory2_QueryInterface(IUnknown* iface, REFIID iid, void** ppvObject)
{
	MILFactory2* This = impl_from_IUnknown(iface);

	if (IsEqualIID(&IID_IUnknown, iid))
	{
		*ppvObject = &This->IUnknown_iface;
	}
	else
	{
		*ppvObject = NULL;
		return E_NOINTERFACE;
	}

	IUnknown_AddRef((IUnknown*)*ppvObject);
	return S_OK;
}

static ULONG WINAPI MILFactory2_AddRef(IUnknown* iface)
{
	MILFactory2* This = impl_from_IUnknown(iface);
	return InterlockedIncrement(&This->ref);
}

static ULONG WINAPI MILFactory2_Release(IUnknown* iface)
{
	MILFactory2* This = impl_from_IUnknown(iface);
	ULONG ref = InterlockedDecrement(&This->ref);

	if (ref == 0)
	{
		free(This);
	}

	return ref;
}

static IUnknownVtbl MILFactory2_Vtbl = {
	MILFactory2_QueryInterface,
	MILFactory2_AddRef,
	MILFactory2_Release
};

HRESULT WINAPI MILCreateFactory(MILFactory2** result, UINT sdkversion)
{
	MILFactory2* factory;

	WINE_TRACE("%p,%x\n", result, sdkversion);

	factory = malloc(sizeof(*factory));
	if (!factory)
		return E_OUTOFMEMORY;

	factory->IUnknown_iface.lpVtbl = &MILFactory2_Vtbl;
	factory->ref = 1;

	*result = factory;
	return S_OK;
}
