// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//+-----------------------------------------------------------------------------
//

//
//  $TAG ENGR

//      $Module:    win_mil_graphics_targets
//      $Keywords:
//
//  $Description:
//      Contains CDummyRenderTarget which implements do nothing
//      IRenderTargetInternal, IMILRenderTargetBitmap, and IWGXBitmapSource
//
//  $ENDTAG
//
//------------------------------------------------------------------------------

class CDummyRenderTarget:
    public IRenderTargetInternal,
    public IMILRenderTargetBitmap,
    public IMILRenderTargetHWND,
    public IWGXBitmapSource
{

protected:

    //
    // There is one static instance of this class which held by this class
    // directly.
    //

    CDummyRenderTarget();

public:

    // Reference to static instance of the dummy rt

    static CDummyRenderTarget * const sc_pDummyRT;
//    const static (CDummyRenderTarget &) sc_dummyRT;
    typedef CDummyRenderTarget &CDummyRenderTargetRef;
    static CDummyRenderTargetRef sc_dummyRTRef;


    // IUnknown.

    //
    // This class should only be created as a static object
    // and as such should never really be reference counted.
    //

    STDMETHODIMP_(ULONG) AddRef(void);
    STDMETHODIMP_(ULONG) Release(void);
    STDMETHODIMP QueryInterface(
        __in_ecount(1) REFIID riid,
        __deref_out void **ppv
        );

    // IMILRenderTarget.

    STDMETHOD_(VOID, GetBounds)(
        __out_ecount(1) MilRectF * const pBounds
        );

    STDMETHOD(Clear)(
        __in_ecount_opt(1) const MilColorF *pColor,
        __in_ecount_opt(1) const CAliasedClip *pAliasedClip
        );

    STDMETHOD(Begin3D)(
        __in_ecount(1) MilRectF const &rcBounds,
        MilAntiAliasMode::Enum AntiAliasMode,
        bool fUseZBuffer,
        FLOAT rZ
        );

    STDMETHOD(End3D)();

    // IRenderTargetInternal.

    STDMETHOD_(__outro_ecount(1) const CMILMatrix *, GetDeviceTransform)() const;

    STDMETHOD(DrawBitmap)(
        __inout_ecount(1) CContextState *pContextState,
        __inout_ecount(1) IWGXBitmapSource *pIBitmap,
        __inout_ecount_opt(1) IMILEffectList *pIEffect
        );

    STDMETHOD(DrawMesh3D)(
        __inout_ecount(1) CContextState* pContextState,
        __inout_ecount_opt(1) BrushContext *pBrushContext,
        __inout_ecount(1) CMILMesh3D *pMesh3D,
        __inout_ecount_opt(1) CMILShader *pShader,
        __inout_ecount_opt(1) IMILEffectList *pIEffect
        );

    STDMETHOD(DrawPath)(
        __inout_ecount(1) CContextState *pContextState,
        __inout_ecount_opt(1) BrushContext *pBrushContext,
        __inout_ecount(1) IShapeData *pShape,
        __inout_ecount_opt(1) CPlainPen *pPen,
        __inout_ecount_opt(1) CBrushRealizer *pStrokeBrush,
        __inout_ecount_opt(1) CBrushRealizer *pFillBrush
        );

    STDMETHOD(DrawInfinitePath)(
        __inout_ecount(1) CContextState *pContextState,
        __inout_ecount(1) BrushContext *pBrushContext,
        __inout_ecount(1) CBrushRealizer *pFillBrush
        ); 

    STDMETHOD(ComposeEffect)(
        __inout_ecount(1) CContextState *pContextState,
        __in_ecount(1) CMILMatrix *pScaleTransform,
        __inout_ecount(1) CMilEffectDuce* pEffect,
        UINT uIntermediateWidth,
        UINT uIntermediateHeight,
        __in_opt IMILRenderTargetBitmap* pImplicitInput
        );
    
    STDMETHOD(DrawGlyphs)(DrawGlyphsParameters &pars);

    STDMETHOD(CreateRenderTargetBitmap)(
        UINT width,
        UINT height,
        IntermediateRTUsage usageInfo,
        MilRTInitialization::Flags dwFlags,
        __deref_out_ecount(1) IMILRenderTargetBitmap **ppIRenderTargetBitmap,
        __in_opt DynArray<bool> const *pActiveDisplays = NULL
        );

    STDMETHOD(BeginLayer)(
        __in_ecount(1) MilRectF const &LayerBounds,
        MilAntiAliasMode::Enum AntiAliasMode,
        __in_ecount_opt(1) IShapeData const *pGeometricMask,
        __in_ecount_opt(1) CMILMatrix const *pGeometricMaskToTarget,
        FLOAT flAlphaScale,
        __in_ecount_opt(1) CBrushRealizer *pAlphaMask
        );

    STDMETHOD(EndLayer)();

    STDMETHOD_(void, EndAndIgnoreAllLayers)();

    STDMETHOD(ReadEnabledDisplays) (
        __inout DynArray<bool> *pEnabledDisplays
        );
    
    // This method is used to determine if the render target is being
    // used to render, or if it's merely being used for bounds accumulation,
    // hit test, etc.
    STDMETHOD(GetType) (__out DWORD *pRenderTargetType) 
    { 
        *pRenderTargetType = DummyRenderTarget; 
        RRETURN(S_OK);
    }

    // This method is used to allow a developer to force ClearType use in
    // intermediate render targets with alpha channels.
    STDMETHOD(SetClearTypeHint)(
        __in bool forceClearType
        )
    {
        RRETURN(S_OK);
    }

    UINT GetRealizationCacheIndex();

    STDMETHOD(DrawVideo)(
        __inout_ecount(1) CContextState *pContextState,
        __inout_ecount(1) IAVSurfaceRenderer *pSurfaceRenderer,
        __inout_ecount(1) IWGXBitmapSource *pBitmapSource,        
        __inout_ecount_opt(1) IMILEffectList *pIEffect
        );

    // IMILRenderTargetBitmap.

    STDMETHOD(GetBitmapSource)(
        __deref_out_ecount(1) IWGXBitmapSource ** const ppIBitmapSource
        );

    STDMETHOD(GetCacheableBitmapSource)(
        __deref_out_ecount(1) IWGXBitmapSource ** const ppIBitmapSource
        );    

    STDMETHOD(GetBitmap)(
        __deref_out_ecount(1) IWGXBitmap ** const ppIBitmap
        );

    // IMILRenderTargetHWND

    STDMETHOD(SetPosition)(
        __in_ecount(1) MilRectF const *prc
        );

    STDMETHOD(GetInvalidRegions)(
        __deref_outro_ecount(*pNumRegions) MilRectF const ** const prgRegions,
        __out_ecount(1) UINT *pNumRegions,
        __out bool *fWholeTargetInvalid
        );

    STDMETHOD(UpdatePresentProperties)(
        MilTransparency::Flags transparencyFlags,
        FLOAT constantAlpha,
        __in_ecount(1) MilColorF const &colorKey
        );

    STDMETHODIMP Present(
        );

    STDMETHODIMP ScrollBlt (
        THIS_
        __in_ecount(1) const RECT *prcSource,
        __in_ecount(1) const RECT *prcDest
        );        

    STDMETHODIMP Invalidate(
        __in_ecount_opt(1) MilRectF const *prc
        );

    STDMETHOD_(VOID, GetIntersectionWithDisplay)(
        UINT iDisplay,
        __out_ecount(1) MilRectL &rcIntersection
        );
    
    STDMETHOD(WaitForVBlank)();

    STDMETHOD_(VOID, AdvanceFrame)(
        UINT uFrameNumber
        );

    STDMETHOD(GetNumQueuedPresents)(
        __out_ecount(1) UINT *puNumQueuedPresents
        );

    STDMETHOD_(bool, CanReuseForThisFrame)(
        THIS_
        __in_ecount(1) IRenderTargetInternal* pIRTParent
        );

    STDMETHOD(CanAccelerateScroll)(
        __out_ecount(1) bool *fCanAccelerateScroll
        );

    // IWGXBitmapSource.

    STDMETHOD(GetSize)(
        __out_ecount(1) UINT *puWidth,
        __out_ecount(1) UINT *puHeight
        );

    STDMETHOD(GetPixelFormat)(
        __out_ecount(1) MilPixelFormat::Enum *pPixelFormat
        );

    STDMETHOD(GetResolution)(
        __out_ecount(1) double *pDpiX,
        __out_ecount(1) double *pDpiY
        );

    STDMETHOD(CopyPalette)(
        __inout_ecount(1) IWICPalette *pIPalette
        );

    STDMETHOD(CopyPixels)(
        __in_ecount_opt(1) const MILRect *prc,
        __in UINT cbStride,
        __in UINT cbBufferSize,
        __out_ecount(cbBufferSize) BYTE *pvPixels
        );

private:

    // internal data
    CMILMatrix m_matDeviceTransform;;

};



