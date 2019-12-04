// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace MS.Internal.Text.TextInterface
{
internal class DWriteTextAnalysisNode<T>
{
	internal T Value;
	internal uint Range_0;
	internal uint Range_1;
	internal DWriteTextAnalysisNode<T> Next;
	internal uint this[uint i]
	{
		get {
			switch(i)
			{
			case 0:
				return Range_0;
			case 1:
				return Range_1;
			default:
				throw new IndexOutOfRangeException();
			}
		}
		set {
			switch(i)
			{
			case 0:
				Range_0 = value;
				break;
			case 1:
				Range_1 = value;
				break;
			default:
				throw new IndexOutOfRangeException();
			}
		}
	}
}

public class TextItemizer
{
	DWriteTextAnalysisNode<DWriteScriptAnalysis> _pScriptAnalysisListHead;
	DWriteTextAnalysisNode<IDWriteNumberSubstitution> _pNumberSubstitutionListHead;

	List<bool> _isDigitList;
	List<uint[]> _isDigitListRanges;

	internal TextItemizer(
		DWriteTextAnalysisNode<DWriteScriptAnalysis> pScriptAnalysisListHead,
		DWriteTextAnalysisNode<IDWriteNumberSubstitution> pNumberSubstitutionListHead)
    {
        _pScriptAnalysisListHead     = pScriptAnalysisListHead;
        _pNumberSubstitutionListHead = pNumberSubstitutionListHead;

        _isDigitList           = new List<bool>();
        _isDigitListRanges     = new List<uint[]>();
    }

    uint GetNextSmallestPos(
        ref DWriteTextAnalysisNode<DWriteScriptAnalysis> ppScriptAnalysisCurrent, 
        ref uint scriptAnalysisRangeIndex,
        ref DWriteTextAnalysisNode<IDWriteNumberSubstitution> ppNumberSubstitutionCurrent,
        ref uint numberSubstitutionRangeIndex,
        ref uint isDigitIndex, 
        ref uint isDigitRangeIndex
        )
    {
        
        uint scriptAnalysisPos = (ppScriptAnalysisCurrent != null)?ppScriptAnalysisCurrent[scriptAnalysisRangeIndex] : UInt32.MaxValue;
        uint numberSubPos      = (ppNumberSubstitutionCurrent != null)?ppNumberSubstitutionCurrent[numberSubstitutionRangeIndex] : UInt32.MaxValue;
        uint isDigitPos        = (isDigitIndex < (uint)_isDigitListRanges.Count)?_isDigitListRanges[(int)isDigitIndex][isDigitRangeIndex] : UInt32.MaxValue;

        uint smallestPos = Math.Min(scriptAnalysisPos, numberSubPos);
        smallestPos = Math.Min(smallestPos, isDigitPos);
        if (smallestPos == scriptAnalysisPos)
        {
            if ((scriptAnalysisRangeIndex + 1) / 2 == 1)
            {
                ppScriptAnalysisCurrent = ppScriptAnalysisCurrent.Next;
            }
            scriptAnalysisRangeIndex = (scriptAnalysisRangeIndex + 1) % 2;
        }
        else if (smallestPos == numberSubPos)
        {                  
            if ((numberSubstitutionRangeIndex + 1) / 2 == 1)
            {
                ppNumberSubstitutionCurrent = ppNumberSubstitutionCurrent.Next;
            }
            numberSubstitutionRangeIndex = (numberSubstitutionRangeIndex + 1) % 2;
        }
        else
        {         
            isDigitIndex     += (isDigitRangeIndex + 1) / 2;
            isDigitRangeIndex = (isDigitRangeIndex + 1) % 2;            
        }
        return smallestPos;

    }

    public IList<Span> Itemize(CultureInfo numberCulture, CharAttribute[] pCharAttribute)
    {
        DWriteTextAnalysisNode<DWriteScriptAnalysis>     pScriptAnalysisListPrevious     = _pScriptAnalysisListHead;
        DWriteTextAnalysisNode<DWriteScriptAnalysis>     pScriptAnalysisListCurrent      = _pScriptAnalysisListHead;
        uint scriptAnalysisRangeIndex = 0;
		int textLength = pCharAttribute.Length;

        DWriteTextAnalysisNode<IDWriteNumberSubstitution> pNumberSubstitutionListPrevious = _pNumberSubstitutionListHead;
        DWriteTextAnalysisNode<IDWriteNumberSubstitution> pNumberSubstitutionListCurrent  = _pNumberSubstitutionListHead;
        uint numberSubstitutionRangeIndex = 0;      

        uint isDigitIndex      = 0;
        uint isDigitIndexOld   = 0;
        uint isDigitRangeIndex = 0;

        uint rangeStart;
    	uint rangeEnd;

        rangeEnd = GetNextSmallestPos(ref pScriptAnalysisListCurrent, ref scriptAnalysisRangeIndex, 
                                      ref pNumberSubstitutionListCurrent, ref numberSubstitutionRangeIndex,
                                      ref isDigitIndex, ref isDigitRangeIndex);

        List<Span> spanVector = new List<Span>();
        while (
            rangeEnd != textLength 
            && (pScriptAnalysisListCurrent != null
            || pNumberSubstitutionListCurrent != null
            || isDigitIndex            < (uint)_isDigitList.Count)
            )
        {
            rangeStart = rangeEnd;
            while(rangeEnd == rangeStart)
            {
                pScriptAnalysisListPrevious     = pScriptAnalysisListCurrent;
                pNumberSubstitutionListPrevious = pNumberSubstitutionListCurrent;
                isDigitIndexOld                 = isDigitIndex;

                rangeEnd = GetNextSmallestPos(ref pScriptAnalysisListCurrent, ref scriptAnalysisRangeIndex,
                                              ref pNumberSubstitutionListCurrent, ref numberSubstitutionRangeIndex,
                                              ref isDigitIndex, ref isDigitRangeIndex);
            }

            IDWriteNumberSubstitution pNumberSubstitution = null;
            if (pNumberSubstitutionListPrevious != null
             && rangeEnd >  pNumberSubstitutionListPrevious.Range_0 
             && rangeEnd <= pNumberSubstitutionListPrevious.Range_1)
            {
                pNumberSubstitution = pNumberSubstitutionListPrevious.Value;
            }

            // Assign HasCombiningMark
            bool hasCombiningMark = false;
            for (uint i = rangeStart; i < rangeEnd; ++i)
            {
                if ((pCharAttribute[i] & CharAttribute.IsCombining) != 0)
                {
                    hasCombiningMark = true;
                    break;
                }
            }

            // Assign NeedsCaretInfo
            // When NeedsCaretInfo is false (and the run does not contain any combining marks)
            // this makes caret navigation happen on the character level 
            // and not the cluster level. When we have an itemized run based on DWrite logic
            // that contains more than one WPF 3.5 scripts (based on unicode 3.x) we might run
            // into a rare scenario where one script allows, for example, ligatures and the other not.
            // In that case we default to false and let the combining marks check (which checks for
            // simple and complex combining marks) decide whether character or cluster navigation
            // will happen for the current run.
            bool needsCaretInfo = true;
            for (uint i = rangeStart; i < rangeEnd; ++i)
            {
                // Does NOT need caret info
                if (((pCharAttribute[i] & CharAttribute.IsStrong) != 0) && ((pCharAttribute[i] & CharAttribute.NeedsCaretInfo) == 0))
                {
                    needsCaretInfo = false;
                    break;
                }
            }

            int strongCharCount = 0;
            int latinCount = 0;
            int indicCount = 0;
            bool hasExtended = false;
            for (uint i = rangeStart; i < rangeEnd; ++i)
            {
                if ((pCharAttribute[i] & CharAttribute.IsExtended) != 0)
                {
                    hasExtended = true;
                }
                

                // If the current character class is Strong.
                if ((pCharAttribute[i] & CharAttribute.IsStrong) != 0)
                {
                    strongCharCount++;

                    if ((pCharAttribute[i] & CharAttribute.IsLatin) != 0)
                    {
                        latinCount++;
                    }
                    else if((pCharAttribute[i] & CharAttribute.IsIndic) != 0)
                    {
                        indicCount++;
                    }
                }
            }

            // Assign isIndic
            // For the isIndic check we mark the run as Indic if it contains atleast
            // one strong Indic character based on the old WPF 3.5 script ids.
            // The isIndic flag is eventually used by LS when checking for the max cluster
            // size that can form for the current run so that it can break the line properly.
            // So our approach is conservative. 1 strong Indic character will make us 
            // communicate to LS the max cluster size possible for correctness.
            bool isIndic = (indicCount > 0);

            // Assign isLatin
            // We mark a run to be Latin iff all the strong characters in it
            // are Latin based on the old WPF 3.5 script ids.
            // This is a conservative approach for correct line breaking behavior.
            // Refer to the comment about isIndic above.
            bool isLatin = (strongCharCount > 0) && (latinCount == strongCharCount);

            ItemProps itemProps = new ItemProps(
                    pScriptAnalysisListPrevious.Value,
                    pNumberSubstitution,
                    _isDigitList[(int)isDigitIndexOld] ? numberCulture : null,
                    hasCombiningMark,
                    needsCaretInfo,
                    hasExtended,
                    isIndic,
                    isLatin
                    );

            spanVector.Add(new Span(itemProps, (int)(rangeEnd - rangeStart)));
        }

        return spanVector;
    }  

    internal void SetIsDigit(
                                 uint textPosition,
                                 uint textLength,
                                 bool   isDigit
                                 )
    {
        _isDigitList.Add(isDigit);
        uint[] range = new uint[2];
        range[0] = textPosition;
        range[1] = textPosition + textLength;
        _isDigitListRanges.Add(range);
    }
}
}
