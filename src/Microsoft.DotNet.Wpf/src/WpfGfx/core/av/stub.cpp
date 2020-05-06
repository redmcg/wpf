// Stubs for things that rely on WMP

#ifndef _MSC_VER
// Wine Mono hack: There are way too many "overrides" in the wrong place, just ignore them.
#define override
#endif

#include "std.h"
#include <d3d9.h>
#include <mfidl.h>
#include <dxva2api.h>
#include "av.h"
#include "avloader.h"
#include "internal.h"

HRESULT CAVLoader::Startup()
{
	return E_NOTIMPL;
}

void CAVLoader::Shutdown()
{
}

HRESULT AvDllInitialize(void)
{
	return E_NOTIMPL;
}

void AvDllShutdown(void)
{
}

HRESULT
CMILAV::
CreateMedia(
    __in        CEventProxy *pEventProxy,
    __in        bool        canOpenAnyMedia,
    __deref_out IMILMedia   **ppMedia
    )
{
	return E_NOTIMPL;
}
