
#define COBJMACROS
#include <windows.h>
#include <initguid.h>
#include "wine/dwrite.h"

typedef struct _factory* HFACTORY; // Opaque pointer-sized handle type

typedef HRESULT CALLBACK (*CreateEnumeratorFromKeyFn)(HFACTORY managedFactoryHandle, 
    void const* collectionKey, UINT collectionKeySize, IDWriteFontFileEnumerator** enumerator);

typedef HRESULT CALLBACK (*CreateStreamFromKeyFn)(HFACTORY managedFactoryHandle, 
    void const* fontFileReferenceKey, UINT fontFileReferenceKeySize,
    IDWriteFontFileStream** fontFileStream);

typedef struct DWriteLoaders {
    LONG ref;
    HFACTORY hfactory;
    IDWriteFactory* dwrite_factory;
    IDWriteFontCollectionLoader collection_loader;
    IDWriteFontFileLoader file_loader;
	CreateEnumeratorFromKeyFn create_enum_fn;
	CreateStreamFromKeyFn create_stream_fn;
} DWriteLoaders;

static inline DWriteLoaders* impl_from_IDWriteFontCollectionLoader(IDWriteFontCollectionLoader* iface)
{
	return CONTAINING_RECORD(iface, DWriteLoaders, collection_loader);
}

static inline DWriteLoaders* impl_from_IDWriteFontFileLoader(IDWriteFontFileLoader* iface)
{
	return CONTAINING_RECORD(iface, DWriteLoaders, file_loader);
}

static ULONG DWriteLoaders_Release(DWriteLoaders* This)
{
	ULONG ref = InterlockedDecrement(&This->ref);

	if (ref == 0)
		free(This);

	return ref;
}

static HRESULT WINAPI CollectionLoader_QueryInterface(IDWriteFontCollectionLoader* iface,
	REFIID iid, void **ppv)
{
	if (IsEqualIID(&IID_IUnknown, iid) || IsEqualIID(&IID_IDWriteFontCollectionLoader, iid))
	{
		IDWriteFontCollectionLoader_AddRef(iface);
		*ppv = iface;
		return S_OK;
	}
	return E_NOINTERFACE;
}

static ULONG WINAPI CollectionLoader_AddRef(IDWriteFontCollectionLoader* iface)
{
	DWriteLoaders* This = impl_from_IDWriteFontCollectionLoader(iface);
	return InterlockedIncrement(&This->ref);
}

static ULONG WINAPI CollectionLoader_Release(IDWriteFontCollectionLoader* iface)
{
	DWriteLoaders* This = impl_from_IDWriteFontCollectionLoader(iface);
	return DWriteLoaders_Release(This);
}

static HRESULT WINAPI CollectionLoader_CreateEnumeratorFromKey(
	IDWriteFontCollectionLoader* iface, IDWriteFactory* factory,
	void const* key, UINT32 key_size, IDWriteFontFileEnumerator **enumerator)
{
	DWriteLoaders* This = impl_from_IDWriteFontCollectionLoader(iface);
	HFACTORY hfactory = This->hfactory;

	if (hfactory == NULL)
		return E_FAIL;
	
	return This->create_enum_fn(hfactory, key, key_size, enumerator);
}

static IDWriteFontCollectionLoaderVtbl CollectionLoader_Vtbl = {
	CollectionLoader_QueryInterface,
	CollectionLoader_AddRef,
	CollectionLoader_Release,
	CollectionLoader_CreateEnumeratorFromKey
};

static HRESULT WINAPI FileLoader_QueryInterface(IDWriteFontFileLoader* iface,
	REFIID iid, void **ppv)
{
	if (IsEqualIID(&IID_IUnknown, iid) || IsEqualIID(&IID_IDWriteFontFileLoader, iid))
	{
		IDWriteFontFileLoader_AddRef(iface);
		*ppv = iface;
		return S_OK;
	}
	return E_NOINTERFACE;
}

static ULONG WINAPI FileLoader_AddRef(IDWriteFontFileLoader* iface)
{
	DWriteLoaders* This = impl_from_IDWriteFontFileLoader(iface);
	return InterlockedIncrement(&This->ref);
}

static ULONG WINAPI FileLoader_Release(IDWriteFontFileLoader* iface)
{
	DWriteLoaders* This = impl_from_IDWriteFontFileLoader(iface);
	return DWriteLoaders_Release(This);
}

static HRESULT WINAPI FileLoader_CreateStreamFromKey(
	IDWriteFontFileLoader* iface,
	void const* key, UINT32 key_size, IDWriteFontFileStream **stream)
{
	DWriteLoaders* This = impl_from_IDWriteFontFileLoader(iface);
	HFACTORY hfactory = This->hfactory;

	if (hfactory == NULL)
		return E_FAIL;
	
	return This->create_stream_fn(hfactory, key, key_size, stream);
}

static IDWriteFontFileLoaderVtbl FileLoader_Vtbl = {
	FileLoader_QueryInterface,
	FileLoader_AddRef,
	FileLoader_Release,
	FileLoader_CreateStreamFromKey
};

void WINAPI ReleaseRegisteredLoaders(DWriteLoaders* This)
{
	IDWriteFactory_UnregisterFontCollectionLoader(This->dwrite_factory, &This->collection_loader);
	IDWriteFactory_UnregisterFontFileLoader(This->dwrite_factory, &This->file_loader);
	This->dwrite_factory = NULL;
	This->hfactory = 0;
}

DWriteLoaders* WINAPI RegisterLoaders(IDWriteFactory* dwrite_factory,
	CreateEnumeratorFromKeyFn enum_fn, CreateStreamFromKeyFn stream_fn,
	HFACTORY hfactory)
{
	DWriteLoaders* This;
	HRESULT hr;

	This = malloc(sizeof(*This));
	This->ref = 1;
	This->hfactory = hfactory;
	IDWriteFactory_AddRef(dwrite_factory);
	This->dwrite_factory = dwrite_factory;
	This->collection_loader.lpVtbl = &CollectionLoader_Vtbl;
	This->file_loader.lpVtbl = &FileLoader_Vtbl;
	This->create_enum_fn = enum_fn;
	This->create_stream_fn = stream_fn;

	hr = IDWriteFactory_RegisterFontCollectionLoader(dwrite_factory, &This->collection_loader);

	if (SUCCEEDED(hr))
		hr = IDWriteFactory_RegisterFontFileLoader(dwrite_factory, &This->file_loader);
	
	if (FAILED(hr))
	{
		ReleaseRegisteredLoaders(This);
		return NULL;
	}

	return This;
}

IDWriteFontFileLoader* GetDWriteFileLoader(DWriteLoaders* This)
{
	return &This->file_loader;
}

IDWriteFontCollectionLoader* GetDWriteCollectionLoader(DWriteLoaders* This)
{
	return &This->collection_loader;
}

