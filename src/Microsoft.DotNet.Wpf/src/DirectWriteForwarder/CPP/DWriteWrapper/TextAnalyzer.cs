// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Media;

namespace MS.Internal.Text.TextInterface
{
public class TextAnalyzer
{
	IDWriteTextAnalyzer _textAnalyzer;

    public TextAnalyzer(IDWriteTextAnalyzer textAnalyzer)
    {
        _textAnalyzer = textAnalyzer;
    }

/* requires Factory
    IList<Span^>^ TextAnalyzer::Itemize(
        __in_ecount(length) const WCHAR* text,
        UINT32                           length,
        CultureInfo^                     culture,
        Factory^                         factory,
        bool                             isRightToLeftParagraph,
        CultureInfo^                     numberCulture,
        bool                             ignoreUserOverride,
        UINT32                           numberSubstitutionMethod,
        IClassification^                 classificationUtility,
        CreateTextAnalysisSink^          pfnCreateTextAnalysisSink,
        GetScriptAnalysisList^           pfnGetScriptAnalysisList,
        GetNumberSubstitutionList^       pfnGetNumberSubstitutionList,
        CreateTextAnalysisSource^        pfnCreateTextAnalysisSource
        )
    {
        // If a text has zero length then we do not need to itemize.
        if (length > 0)
        {
            IDWriteTextAnalyzer* pTextAnalyzer = NULL;
            IDWriteTextAnalysisSink* pTextAnalysisSink = NULL;
            IDWriteTextAnalysisSource* pTextAnalysisSource = NULL;

            // We obtain an AddRef factory so as not to worry about having to call GC::KeepAlive(factory)
            // which puts unnecessary maintenance cost on this code.
            IDWriteFactory* pDWriteFactory = factory->DWriteFactoryAddRef;
            HRESULT hr = S_OK;
            try
            {
                hr = pDWriteFactory->CreateTextAnalyzer(&pTextAnalyzer);
                ConvertHresultToException(hr, "List<Span^>^ TextAnalyzer::Itemize");
                
                pin_ptr<const WCHAR> pNumberSubstitutionLocaleNamePinned;

                // We need this variable since we cannot assign NULL to a pin_ptr<const WCHAR>.
                WCHAR const* pNumberSubstitutionLocaleName = NULL;
                if (numberCulture != nullptr)
                {
                    pNumberSubstitutionLocaleNamePinned = Native::Util::GetPtrToStringChars(numberCulture->IetfLanguageTag);
                    pNumberSubstitutionLocaleName = pNumberSubstitutionLocaleNamePinned;
                }

                pin_ptr<const WCHAR> pCultureName = Native::Util::GetPtrToStringChars(culture->IetfLanguageTag);
                
                // NOTE: the text parameter is NOT copied inside TextAnalysisSource to improve perf.
                // This is ok as long as we use the TextAnalysisSource in the same scope as we hold ref to text.
                // If we are ever to change this pattern then this should be revisited in TextAnalysisSource in
                // PresentationNative.
                hr = static_cast<HRESULT>(pfnCreateTextAnalysisSource(text,
                                                                      length,
                                                                      pCultureName,
                                                                      (void*)(pDWriteFactory),
                                                                      isRightToLeftParagraph,
                                                                      pNumberSubstitutionLocaleName,
                                                                      ignoreUserOverride,
                                                                      numberSubstitutionMethod,
                                                                      (void**)&pTextAnalysisSource));
                ConvertHresultToException(hr, "List<Span^>^ TextAnalyzer::Itemize");

            
                pTextAnalysisSink = (IDWriteTextAnalysisSink*)pfnCreateTextAnalysisSink();

                // Analyze the script ranges.
                hr = pTextAnalyzer->AnalyzeScript(pTextAnalysisSource,
                                                 0,
                                                 length,
                                                 pTextAnalysisSink);
                ConvertHresultToException(hr, "List<Span^>^ TextAnalyzer::Itemize");

                // Analyze the number substitution ranges.
                hr = pTextAnalyzer->AnalyzeNumberSubstitution(pTextAnalysisSource,
                                                            0,
                                                            length,
                                                            pTextAnalysisSink);
                ConvertHresultToException(hr, "List<Span^>^ TextAnalyzer::Itemize");

                DWriteTextAnalysisNode<DWRITE_SCRIPT_ANALYSIS>*     dwriteScriptAnalysisNode     = (DWriteTextAnalysisNode<DWRITE_SCRIPT_ANALYSIS>*)pfnGetScriptAnalysisList((void*)pTextAnalysisSink);
                DWriteTextAnalysisNode<IDWriteNumberSubstitution*>* dwriteNumberSubstitutionNode = (DWriteTextAnalysisNode<IDWriteNumberSubstitution*>*)pfnGetNumberSubstitutionList((void*)pTextAnalysisSink);
                
                TextItemizer^ textItemizer = gcnew TextItemizer(dwriteScriptAnalysisNode, dwriteNumberSubstitutionNode);
            
                return AnalyzeExtendedAndItemize(textItemizer, text, length, numberCulture, classificationUtility);
            }
            finally
            {
                ReleaseItemizationNativeResources(&pDWriteFactory,
                                                  &pTextAnalyzer,
                                                  &pTextAnalysisSource,
                                                  &pTextAnalysisSink);
            }
        }
        else
        {
            return nullptr;
        }

    }
*/

    static IList<Span> AnalyzeExtendedAndItemize(
        TextItemizer textItemizer, 
        IntPtr text, 
        uint length, 
        CultureInfo numberCulture, 
        IClassification classification
        )
    {
        Debug.Assert(length >= 0);

        CharAttribute[] pCharAttribute = new CharAttribute[length];

		// Analyze the extended character ranges.
		AnalyzeExtendedCharactersAndDigits(text, length, textItemizer, pCharAttribute, numberCulture, classification);
		return textItemizer.Itemize(numberCulture, pCharAttribute);
    }

    public static void AnalyzeExtendedCharactersAndDigits(
        IntPtr                           text,
        uint                             length,
        TextItemizer                     textItemizer,
        CharAttribute[] 				 pCharAttribute,
        CultureInfo                      numberCulture,
        IClassification                  classificationUtility
        )
    {
        // Text will never be of zero length. This is enforced by Itemize().
        bool isCombining;
        bool needsCaretInfo;
        bool isIndic;
        bool isDigit;
        bool isLatin;
        bool isStrong;
        bool isExtended;

		char ch = (char)Marshal.ReadInt16(text);

        classificationUtility.GetCharAttribute(
            ch,
            out isCombining,
            out needsCaretInfo,
            out isIndic,
            out isDigit,
            out isLatin,
            out isStrong
            );

        isExtended = ItemizerHelper.IsExtendedCharacter(ch);

        uint isDigitRangeStart = 0;
        uint isDigitRangeEnd = 0;
        bool   previousIsDigitValue = (numberCulture == null) ? false : isDigit;
        bool   currentIsDigitValue;

        // pCharAttribute is assumed to have the same length as text. This is enforced by Itemize().
        pCharAttribute[0] = (CharAttribute)
                            (((isCombining)    ? CharAttribute.IsCombining    : CharAttribute.None)
                           | ((needsCaretInfo) ? CharAttribute.NeedsCaretInfo : CharAttribute.None)
                           | ((isLatin)        ? CharAttribute.IsLatin        : CharAttribute.None)
                           | ((isIndic)        ? CharAttribute.IsIndic        : CharAttribute.None)
                           | ((isStrong)       ? CharAttribute.IsStrong       : CharAttribute.None)
                           | ((isExtended)     ? CharAttribute.IsExtended     : CharAttribute.None));

		uint i;
        for (i = 1; i < length; ++i)
        {
			ch = (char)Marshal.ReadInt16(text, 2 * (int)i);

            classificationUtility.GetCharAttribute(
            ch,
            out isCombining,
            out needsCaretInfo,
            out isIndic,
            out isDigit,
            out isLatin,
            out isStrong
            );

            isExtended = ItemizerHelper.IsExtendedCharacter(ch);
            

            pCharAttribute[i] = (CharAttribute)
                                (((isCombining)    ? CharAttribute.IsCombining    : CharAttribute.None)
                               | ((needsCaretInfo) ? CharAttribute.NeedsCaretInfo : CharAttribute.None)
                               | ((isLatin)        ? CharAttribute.IsLatin        : CharAttribute.None)
                               | ((isIndic)        ? CharAttribute.IsIndic        : CharAttribute.None)
                               | ((isStrong)       ? CharAttribute.IsStrong       : CharAttribute.None)
                               | ((isExtended)     ? CharAttribute.IsExtended     : CharAttribute.None));


            currentIsDigitValue = (numberCulture == null) ? false : isDigit;
            if (previousIsDigitValue != currentIsDigitValue)
            {

                isDigitRangeEnd = i;
                textItemizer.SetIsDigit(isDigitRangeStart, isDigitRangeEnd - isDigitRangeStart, previousIsDigitValue);

                isDigitRangeStart = i;
                previousIsDigitValue = currentIsDigitValue;
            }
        }


        isDigitRangeEnd = i;
        textItemizer.SetIsDigit(isDigitRangeStart, isDigitRangeEnd - isDigitRangeStart, previousIsDigitValue);
    }

	public static char CharHyphen = (char)0x2d;

    unsafe void GetBlankGlyphsForControlCharacters(
        IntPtr                                       pTextString,
        uint                                         textLength,
        FontFace                                     fontFace,
        ushort                                       blankGlyphIndex,
        uint                                         maxGlyphCount,
        ushort*                                      clusterMap, /* ushort[textLength] */
        ushort*                                      glyphIndices, /* ushort[maxGlyphCount] */
        int*             							 pfCanGlyphAlone, /* int[textLength] */
        [Out] out uint 								 actualGlyphCount
        )
    {
        actualGlyphCount = textLength;
        // There is not enough buffer allocated. Signal to the caller the correct buffer size.
        if (maxGlyphCount < textLength)
        {
            return;
        }

        if (textLength > UInt16.MaxValue)
        {
			throw new ArgumentException("textLength must be no more than UInt16.MaxValue");
        }

        ushort textLengthShort = (ushort)textLength;

        uint softHyphen = (uint)CharHyphen;
        ushort hyphenGlyphIndex = 0;
        for (ushort i = 0; i < textLengthShort; ++i)
        {
            // LS will sometimes replace soft hyphens (which are invisible) with hyphens (which are visible).
            // So if we are in this code then LS actually did this replacement and we need to display the hyphen (x002D)
            if (Marshal.ReadInt16(pTextString, i*2) == CharHyphen)
            {
                if (hyphenGlyphIndex == 0)
                {
					fontFace.GetArrayOfGlyphIndices(&softHyphen, 1, &hyphenGlyphIndex);
                }
                glyphIndices[i] = hyphenGlyphIndex;
            }
            else
            {
                glyphIndices[i] = blankGlyphIndex;
            }
            clusterMap     [i] = i;
            pfCanGlyphAlone[i] = 1;
        }
    }

    public unsafe void GetGlyphs(
        IntPtr                                       textString,
    	uint                                         textLength,
        Font                                         font,
        ushort                                       blankGlyphIndex,
        bool                                         isSideways,
        bool                                         isRightToLeft,
        CultureInfo                                  cultureInfo,
        DWriteFontFeature[][]                        features,
        uint[]                                       featureRangeLengths,
        uint                                         maxGlyphCount,
        TextFormattingMode                           textFormattingMode,
        ItemProps                                    itemProps,
        ushort*             						 clusterMap, /* textLength */
        ushort*             						 textProps, /* textLength */
        ushort*          							 glyphIndices, /* maxGlyphCount */
        ushort*       								 glyphProps, /* maxGlyphCount */
        int*                						 pfCanGlyphAlone, /* textLength */
        [Out] out uint 								 actualGlyphCount
        )
    {
        // Shaping should not have taken place if ScriptAnalysis is NULL!
    	Debug.Assert(itemProps.ScriptAnalysis != null);

        // These are control characters and WPF will not display control characters.
        if (GetScriptShapes(itemProps) != DWriteScriptShapes.Default)
        {
            FontFace fontFace = font.GetFontFace();
            try
            {
                GetBlankGlyphsForControlCharacters(
                    textString,
                    textLength,
                    fontFace,
                    blankGlyphIndex,
                    maxGlyphCount,
                    clusterMap,
                    glyphIndices,
                    pfCanGlyphAlone,
                    out actualGlyphCount
                    );
            }
            finally
            {
                fontFace.Release();
            }
        }
        else
        {
            string localeName = cultureInfo.IetfLanguageTag;
			var featureRangeLengthsNonNull = (featureRangeLengths != null) ? featureRangeLengths : new uint[0];
			fixed (uint* pFeatureRangeLengthsPinned = featureRangeLengthsNonNull)
			{
				uint* pFeatureRangeLengths = null;

				uint featureRanges = 0;
				GCHandle[] dwriteFontFeaturesGCHandles = null;
				IntPtr[] dwriteTypographicFeatures = null;
				IntPtr dwriteTypographicFeaturesMemory = IntPtr.Zero;

				if (features != null)
				{
					pFeatureRangeLengths = pFeatureRangeLengthsPinned;
					featureRanges = (uint)featureRangeLengths.Length;
					dwriteFontFeaturesGCHandles = new GCHandle[featureRanges];
					dwriteTypographicFeatures = new IntPtr[featureRanges];
					dwriteTypographicFeaturesMemory = Marshal.AllocCoTaskMem(Marshal.SizeOf<DWriteTypographicFeatures>() * (int)featureRanges);
				}

				FontFace fontFace = font.GetFontFace();
				try
				{
					if (features != null)
					{
						for (uint i = 0; i < featureRanges; ++i)
						{
							dwriteFontFeaturesGCHandles[i] = GCHandle.Alloc(features[i], GCHandleType.Pinned);
							var new_feature = new DWriteTypographicFeatures();
							new_feature.features = dwriteFontFeaturesGCHandles[i].AddrOfPinnedObject();
							new_feature.featureCount = features[i].Length;
							dwriteTypographicFeatures[i] = dwriteTypographicFeaturesMemory + (int)i * Marshal.SizeOf<DWriteTypographicFeatures>();
							Marshal.StructureToPtr<DWriteTypographicFeatures>(new_feature, dwriteTypographicFeatures[i], false);
						}
					}


					uint glyphCount = 0;
					IntPtr dwriteScriptAnalysis = itemProps.ScriptAnalysisCoTaskMem();

					int hr = _textAnalyzer.GetGlyphs(
						textString,
						/*checked*/((uint)textLength),
						fontFace.DWriteFontFaceNoAddRef,
						isSideways,
						isRightToLeft,
						dwriteScriptAnalysis,
						localeName,
						itemProps.NumberSubstitution,
						dwriteTypographicFeatures,
						pFeatureRangeLengths,
						featureRanges,
						/*checked*/((uint)maxGlyphCount),
						clusterMap,
						textProps,
						glyphIndices,
						glyphProps,
						out glyphCount
						);

					if (unchecked((int)0x80070057) == hr) // E_INVALIDARG
					{
						// If pLocaleName is unsupported (e.g. "prs-af"), DWrite returns E_INVALIDARG.
						// Try again with the default mapping.
						hr = _textAnalyzer.GetGlyphs(
							textString,
							/*checked*/((uint)textLength),
							fontFace.DWriteFontFaceNoAddRef,
							isSideways,
							isRightToLeft,
							dwriteScriptAnalysis,
							null,
							itemProps.NumberSubstitution,
							dwriteTypographicFeatures,
							pFeatureRangeLengths,
							featureRanges,
							/*checked*/((uint)maxGlyphCount),
							clusterMap,
							textProps,
							glyphIndices,
							glyphProps,
							out glyphCount
							);
					}

					Marshal.FreeCoTaskMem(dwriteScriptAnalysis);

					if (features != null)
					{
						for (uint i = 0; i < featureRanges; ++i)
						{
							dwriteFontFeaturesGCHandles[i].Free();
						}
						Marshal.FreeCoTaskMem(dwriteTypographicFeaturesMemory);
					}

					if (hr == unchecked((int)0x8007007a)) // HRESULT_FROM_WIN32(ERROR_INSUFFICIENT_BUFFER)
					{
						// Actual glyph count is not returned by DWrite unless the call tp GetGlyphs succeeds.
						// It must be re-estimated in case the first estimate was not adequate.
						// The following calculation is a refactoring of DWrite's logic ( 3*stringLength/2 + 16) after 3 retries.
						// We'd rather go directly to the maximum buffer size we are willing to allocate than pay the cost of continuously retrying.
						actualGlyphCount = 27 * maxGlyphCount / 8 + 76;
					}
					else
					{
						Marshal.ThrowExceptionForHR(hr);
						if (pfCanGlyphAlone != null)
						{
							for (uint i = 0; i < textLength; ++i)
							{
								pfCanGlyphAlone[i] = (DWriteBitfieldUtils.ShapingText_IsShapedAlone(textProps[i])) ? 1 : 0;
							}
						}

						actualGlyphCount = glyphCount;
					}
				}
				finally
				{
					fontFace.Release();
				}
			}
		}
	}

    unsafe void GetGlyphPlacementsForControlCharacters(
        IntPtr pTextString,
        uint textLength,
        Font font,
        TextFormattingMode textFormattingMode,
        double fontEmSize,
        double scalingFactor,
        bool isSideways,
        float pixelsPerDip,
        uint glyphCount,
        ushort* pGlyphIndices, /* [in] glyphCount */
        int* glyphAdvances, /* glyphCount */
        [Out] GlyphOffset[] glyphOffsets
        )
    {
        if (glyphCount != textLength)
        {
			throw new ArgumentException("glyphCount must equal textLength");
        }

        glyphOffsets = new GlyphOffset[textLength];
        FontFace fontFace = font.GetFontFace();

        try
        {
            int hyphenAdvanceWidth = -1;
            for (uint i = 0; i < textLength; ++i)
            {
                // LS will sometimes replace soft hyphens (which are invisible) with hyphens (which are visible).
                // So if we are in this code then LS actually did this replacement and we need to display the hyphen (x002D)
                if (Marshal.ReadInt16(pTextString, (int)i*2) == CharHyphen)
                {
                    if (hyphenAdvanceWidth == -1)
                    {
                        GlyphMetrics glyphMetrics;

						if (textFormattingMode == TextFormattingMode.Ideal)
						{
							fontFace.GetDesignGlyphMetrics(pGlyphIndices + i, 1, out glyphMetrics);
						}
						else
						{
							fontFace.GetDisplayGlyphMetrics(
								pGlyphIndices + i,
								1,
								out glyphMetrics,
								(float)fontEmSize,
								textFormattingMode != TextFormattingMode.Display,
								isSideways,
								pixelsPerDip);
						}
                        double approximatedHyphenAW = Math.Round(glyphMetrics.AdvanceWidth * fontEmSize / font.Metrics.DesignUnitsPerEm * pixelsPerDip) / pixelsPerDip;
                        hyphenAdvanceWidth = (int)Math.Round(approximatedHyphenAW * scalingFactor);
                    }

                    glyphAdvances[i] = hyphenAdvanceWidth;
                }
                else
                {
                    glyphAdvances[i] = 0;
                }
                glyphOffsets[i].du = 0;
                glyphOffsets[i].dv = 0;
            }
        }
        finally
        {
            fontFace.Release();
        }
    }

#if DISABLED // requires Factory
    public void GetGlyphPlacements(
        IntPtr                  textString,
        ushort*                 clusterMap, /* textLength */
        ushort*                 textProps, /* textLength */
        uint                    textLength,
        ushort*                 glyphIndices, /* glyphCount */
        uint*                   glyphProps, /* glyphCount */
        uint                    glyphCount,
        Font                    font,
        double                  fontEmSize,
        double                  scalingFactor,
        bool                    isSideways,
        bool                    isRightToLeft,
        CultureInfo             cultureInfo,
        DWriteFontFeature[][]   features,
        uint[]                  featureRangeLengths,
        TextFormattingMode      textFormattingMode,
        ItemProps               itemProps,
        float                   pixelsPerDip,
        int*                    glyphAdvances, /* glyphCount */
        [Out] out GlyphOffset[] glyphOffsets
        )
    {
        // Shaping should not have taken place if ScriptAnalysis is NULL!
    	Debug.Assert(itemProps.ScriptAnalysis != null);

        // These are control characters and WPF will not display control characters.
        if (GetScriptShapes(itemProps) != DWriteScriptShapes.Default)
        {
            GetGlyphPlacementsForControlCharacters(
                textString,
                textLength,
                font,
                textFormattingMode,
                fontEmSize,
                scalingFactor,
                isSideways,
                pixelsPerDip,
                glyphCount,
                glyphIndices,
                glyphAdvances,
                glyphOffsets
                );
        }
        else
        {
            dwriteGlyphAdvances = new float[glyphCount];
            DWriteGlyphOffsets dwriteGlyphOffsets = null;

			var featureRangeLengthsNonNull = featureRangeLengths ? featureRangeLengths : new uint[0];
			fixed (uint* pFeatureRangeLengthsPinned = featureRangeLengthsNonNull)
			{
				GCHandle[] dwriteFontFeaturesGCHandles = null;
				uint featureRanges = 0;
				IntPtr[] dwriteTypographicFeatures = null;
				uint* pFeatureRangeLengths = null;

				if (features != null)
				{
					featureRanges = (uint)featureRangeLengths.Length;
					dwriteTypographicFeatures = new IntPtr[featureRanges];
					dwriteFontFeaturesGCHandles = new GCHandle[featureRanges];
					dwriteTypographicFeatures = new IntPtr[featureRanges];
					dwriteTypographicFeaturesMemory = Marshal.AllocCoTaskMemory(Marshal.Sizeof<DWriteTypographicFeatures>() * featureRanges);
				}

				FontFace fontFace = font.GetFontFace();
				try
				{
					string localeName = cultureInfo.IetfLanguageTag;
					DWRITE_MATRIX transform = Factory::GetIdentityTransform();

					if (features != nullptr)
					{
						for (UINT32 i = 0; i < featureRanges; ++i)
						{
							dwriteFontFeaturesGCHandles[i] = GCHandle::Alloc(features[i], GCHandleType::Pinned);
							dwriteTypographicFeatures[i] = new DWRITE_TYPOGRAPHIC_FEATURES();
							dwriteTypographicFeatures[i]->features = reinterpret_cast<DWRITE_FONT_FEATURE*>(dwriteFontFeaturesGCHandles[i].AddrOfPinnedObject().ToPointer());
							dwriteTypographicFeatures[i]->featureCount = features[i]->Length;
						}
					}

					FLOAT fontEmSizeFloat = (FLOAT)fontEmSize;
					HRESULT hr = E_FAIL;

					if (textFormattingMode == TextFormattingMode::Ideal)
					{   
						hr = _textAnalyzer->Value->GetGlyphPlacements(
							textString,
							clusterMap,
							(DWRITE_SHAPING_TEXT_PROPERTIES*)textProps,
							textLength,
							glyphIndices,
							(DWRITE_SHAPING_GLYPH_PROPERTIES*)glyphProps,
							glyphCount,
							fontFace->DWriteFontFaceNoAddRef,
							fontEmSizeFloat,
							isSideways ? TRUE : FALSE,
							isRightToLeft ? TRUE : FALSE,
							(DWRITE_SCRIPT_ANALYSIS*)(itemProps->ScriptAnalysis),
							localeName,
							(DWRITE_TYPOGRAPHIC_FEATURES const**)dwriteTypographicFeatures,
							pFeatureRangeLengths,
							featureRanges,
							dwriteGlyphAdvances,
							dwriteGlyphOffsets
							);

						if (E_INVALIDARG == hr)
						{
							// If pLocaleName is unsupported (e.g. "prs-af"), DWrite returns E_INVALIDARG.
							// Try again with the default mapping.
							hr = _textAnalyzer->Value->GetGlyphPlacements(
								textString,
								clusterMap,
								(DWRITE_SHAPING_TEXT_PROPERTIES*)textProps,
								textLength,
								glyphIndices,
								(DWRITE_SHAPING_GLYPH_PROPERTIES*)glyphProps,
								glyphCount,
								fontFace->DWriteFontFaceNoAddRef,
								fontEmSizeFloat,
								isSideways ? TRUE : FALSE,
								isRightToLeft ? TRUE : FALSE,
								(DWRITE_SCRIPT_ANALYSIS*)(itemProps->ScriptAnalysis),
								NULL /* default locale mapping */,
								(DWRITE_TYPOGRAPHIC_FEATURES const**)dwriteTypographicFeatures,
								pFeatureRangeLengths,
								featureRanges,
								dwriteGlyphAdvances,
								dwriteGlyphOffsets
								);
						}
						
					}
					else
					{
						assert(textFormattingMode == TextFormattingMode::Display);

						hr = _textAnalyzer->Value->GetGdiCompatibleGlyphPlacements(
							textString,
							clusterMap,
							(DWRITE_SHAPING_TEXT_PROPERTIES*)textProps,
							textLength,
							glyphIndices,
							(DWRITE_SHAPING_GLYPH_PROPERTIES*)glyphProps,
							glyphCount,
							fontFace->DWriteFontFaceNoAddRef,
							fontEmSizeFloat,
							pixelsPerDip,
							&transform,
							FALSE,  // useGdiNatural
							isSideways ? TRUE : FALSE,
							isRightToLeft ? TRUE : FALSE,
							(DWRITE_SCRIPT_ANALYSIS*)(itemProps->ScriptAnalysis),
							pLocaleName,
							(DWRITE_TYPOGRAPHIC_FEATURES const**)dwriteTypographicFeatures,
							pFeatureRangeLengths,
							featureRanges,
							dwriteGlyphAdvances,
							dwriteGlyphOffsets
							);

						if (E_INVALIDARG == hr)
						{
							// If pLocaleName is unsupported (e.g. "prs-af"), DWrite returns E_INVALIDARG.
							// Try again with the default mapping.
							hr = _textAnalyzer->Value->GetGdiCompatibleGlyphPlacements(
								textString,
								clusterMap,
								(DWRITE_SHAPING_TEXT_PROPERTIES*)textProps,
								textLength,
								glyphIndices,
								(DWRITE_SHAPING_GLYPH_PROPERTIES*)glyphProps,
								glyphCount,
								fontFace->DWriteFontFaceNoAddRef,
								fontEmSizeFloat,
								pixelsPerDip,
								&transform,
								FALSE,  // useGdiNatural
								isSideways ? TRUE : FALSE,
								isRightToLeft ? TRUE : FALSE,
								(DWRITE_SCRIPT_ANALYSIS*)(itemProps->ScriptAnalysis),
								NULL /* default locale mapping */,
								(DWRITE_TYPOGRAPHIC_FEATURES const**)dwriteTypographicFeatures,
								pFeatureRangeLengths,
								featureRanges,
								dwriteGlyphAdvances,
								dwriteGlyphOffsets
								);
						}
					}

					System::GC::KeepAlive(fontFace);
					System::GC::KeepAlive(itemProps);
					System::GC::KeepAlive(_textAnalyzer);

					if (features != nullptr)
					{
						for (UINT32 i = 0; i < featureRanges; ++i)
						{
							dwriteFontFeaturesGCHandles[i].Free();
							delete dwriteTypographicFeatures[i];
						}
						FREE dwriteTypographicFeaturesMemory;
					}

					glyphOffsets = gcnew array<GlyphOffset>(glyphCount);
					if (textFormattingMode == TextFormattingMode::Ideal)
					{
						for (UINT32 i = 0; i < glyphCount; ++i)
						{
							glyphAdvances[i] = (int)Math::Round(dwriteGlyphAdvances[i] * fontEmSize * scalingFactor / fontEmSizeFloat);
							glyphOffsets[i].du = (int)(dwriteGlyphOffsets[i].advanceOffset * scalingFactor);
							glyphOffsets[i].dv = (int)(dwriteGlyphOffsets[i].ascenderOffset * scalingFactor);
						}
					}
					else
					{
						for (UINT32 i = 0; i < glyphCount; ++i)
						{
							glyphAdvances[i] = (int)Math::Round(dwriteGlyphAdvances[i] * scalingFactor);
							glyphOffsets[i].du = (int)(dwriteGlyphOffsets[i].advanceOffset * scalingFactor);
							glyphOffsets[i].dv = (int)(dwriteGlyphOffsets[i].ascenderOffset * scalingFactor);
						}
					}                

					ConvertHresultToException(hr, "void TextAnalyzer::GetGlyphs");
				}
				finally
				{
					fontFace->Release();

					if (dwriteGlyphAdvances != NULL)
					{
						delete[] dwriteGlyphAdvances;
					}

					if (dwriteGlyphOffsets != NULL)
					{
						delete[] dwriteGlyphOffsets;
					}

					if (dwriteTypographicFeatures != NULL)
					{
						delete[] dwriteTypographicFeatures;
					}
				}
        }
    }

    public void GetGlyphsAndTheirPlacements(
        IntPtr textString,
        uint textLength,
        Font font,
        ushort blankGlyphIndex,
        bool isSideways,
        bool isRightToLeft,
        CultureInfo cultureInfo,
        DWriteFontFeature[][] features,
        uint[] featureRangeLengths,
        double fontEmSize,
        double scalingFactor,
        float pixelsPerDip,
        TextFormattingMode textFormattingMode,
        ItemProps     itemProps,
        [Out] out ushort[] clusterMap,
        [Out] out ushort[] glyphIndices,
        [Out] out int[] glyphAdvances,
        [Out] out GlyphOffset[] glyphOffsets
        )
    {
        uint maxGlyphCount = 3 * textLength;
        clusterMap = new ushort[textLength];
		ushort[] textProps = new ushort[textLength];
        fixed (ushort* pclusterMapPinned = clusterMap,
				pTextProps = textProps)
		{
			IntPtr glyphProps = IntPtr.Zero; /* uint* */
        DWRITE_SHAPING_GLYPH_PROPERTIES* glyphProps         = NULL;
        unsigned short*                  glyphIndicesNative = NULL;

        try
        {
            UINT32 actualGlyphCount = maxGlyphCount + 1;

            // Loop and everytime increase the size of the GlyphIndices buffer.
            while(actualGlyphCount > maxGlyphCount)
            {
                maxGlyphCount = actualGlyphCount;
                if (glyphProps != NULL)
                {
                    delete[] glyphProps;
                    glyphProps = NULL;
                }
                glyphProps   = new DWRITE_SHAPING_GLYPH_PROPERTIES[maxGlyphCount];

                if (glyphIndicesNative != NULL)
                {
                    delete[] glyphIndicesNative;
                    glyphIndicesNative = NULL;
                }
                glyphIndicesNative = new unsigned short[maxGlyphCount];

                GetGlyphs(
                    textString,
                    textLength,
                    font,
                    blankGlyphIndex,
                    isSideways,
                    isRightToLeft,
                    cultureInfo,
                    features,
                    featureRangeLengths,
                    maxGlyphCount,
                    textFormattingMode,
                    itemProps,
                    reinterpret_cast<UINT16*> (pclusterMapPinned),
                    (UINT16*) textProps,
                    reinterpret_cast<UINT16*> (glyphIndicesNative),
                    reinterpret_cast<UINT32*> (glyphProps),
                    NULL,
                    actualGlyphCount
                    );
            }

            glyphIndices = gcnew array<unsigned short>(actualGlyphCount);
            Marshal::Copy(System::IntPtr((void*)glyphIndicesNative), (array<Int16>^)glyphIndices, 0, actualGlyphCount);

            glyphAdvances = gcnew array<int>(actualGlyphCount);
            pin_ptr<int> glyphAdvancesPinned = &glyphAdvances[0]; 
            glyphOffsets = gcnew array<GlyphOffset>(actualGlyphCount);

            GetGlyphPlacements(
                textString,
                reinterpret_cast<UINT16*> (pclusterMapPinned),
                (UINT16*) textProps,
                textLength,
                reinterpret_cast<UINT16*> (glyphIndicesNative),
                reinterpret_cast<UINT32*> (glyphProps),
                actualGlyphCount,
                font,
                fontEmSize,
                scalingFactor,
                isSideways,
                isRightToLeft,
                cultureInfo,
                features,
                featureRangeLengths,
                textFormattingMode,
                itemProps,
                pixelsPerDip,
                reinterpret_cast<int*>(glyphAdvancesPinned),
                glyphOffsets
                );
        }
        finally
        {
            delete[] textProps;
            if (glyphProps != NULL)
            {
                delete[] glyphProps;
            }
            if (glyphIndicesNative != NULL)
            {
                delete[] glyphIndicesNative;
            }
        }
    }
#endif

    DWriteScriptShapes GetScriptShapes(ItemProps itemProps)
    {
        return ((DWriteScriptAnalysis)itemProps.ScriptAnalysis).Shapes;
    }
}
}
