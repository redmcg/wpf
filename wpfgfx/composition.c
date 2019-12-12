// composition.c: MilCompositionEngine functions

#include <windows.h>
#include <winnt.h>

static const char composition_engine_lock_name[] = "wpfgfx/composition.c: composition_engine_lock";

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
