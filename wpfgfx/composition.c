// composition.c: MilCompositionEngine functions

#include <windows.h>
#include <winnt.h>
#include "wine/debug.h"

WINE_DEFAULT_DEBUG_CHANNEL(wpfgfx);

static CRITICAL_SECTION composition_engine_lock;
static CRITICAL_SECTION_DEBUG composition_engine_lock_debug = {
	0, 0, &composition_engine_lock,
	{ &composition_engine_lock_debug.ProcessLocksList, &composition_engine_lock_debug.ProcessLocksList },
};
static CRITICAL_SECTION composition_engine_lock = { &composition_engine_lock_debug, -1 };

void WINAPI MilCompositionEngine_EnterCompositionEngineLock(void)
{
	EnterCriticalSection(&composition_engine_lock);
}

void WINAPI MilCompositionEngine_ExitCompositionEngineLock(void)
{
	LeaveCriticalSection(&composition_engine_lock);
}

HRESULT WINAPI MilCompositionEngine_InitializePartitionManager(INT nPriority)
{
	// FIXME: Start a thread?
	WINE_TRACE("%i\n", nPriority);
	return S_OK;
}
