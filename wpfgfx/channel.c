// channel.c - Connection/Channel functions

#include <windows.h>

typedef struct _MilChannel {
	void* transport;
} MilChannel;

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
	return S_OK;
}
