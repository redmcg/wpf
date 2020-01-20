// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace MS.Internal.Text.TextInterface
{

public class TextItemizer
{
	List<TextAnalysisRange<DWriteScriptAnalysis>> ScriptAnalysisRanges;
	List<TextAnalysisRange<IDWriteNumberSubstitution>> NumberSubstitutionRanges;
	List<TextAnalysisRange<bool>> IsDigitRanges;

	internal TextItemizer(
		List<TextAnalysisRange<DWriteScriptAnalysis>> ScriptAnalysisRanges,
		List<TextAnalysisRange<IDWriteNumberSubstitution>> NumberSubstitutionRanges)
    {
		this.ScriptAnalysisRanges = ScriptAnalysisRanges;
		this.NumberSubstitutionRanges = NumberSubstitutionRanges;
		this.IsDigitRanges = new List<TextAnalysisRange<bool>> ();
    }

    public IList<Span> Itemize(CultureInfo numberCulture, CharAttribute[] pCharAttribute)
	{
		var result = new List<Span>();
		int textIndex = 0;
		int scriptAnalysisIndex = 0;
		int numberSubstitutionIndex = 0;
		int isDigitIndex = 0;
		
		while (true)
		{
			if (textIndex >= ScriptAnalysisRanges[scriptAnalysisIndex].TextEnd)
			{
				scriptAnalysisIndex++;
				if (scriptAnalysisIndex >= ScriptAnalysisRanges.Count)
					break;
			}

			int spanEnd = ScriptAnalysisRanges[scriptAnalysisIndex].TextEnd;

			var scriptAnalysis = ScriptAnalysisRanges[scriptAnalysisIndex].Value;

			IDWriteNumberSubstitution numberSubstitution = null;
			if (numberSubstitutionIndex < NumberSubstitutionRanges.Count)
			{
				if (textIndex >= NumberSubstitutionRanges[numberSubstitutionIndex].TextEnd)
				{
					numberSubstitutionIndex++;
				}

				if (numberSubstitutionIndex < NumberSubstitutionRanges.Count)
				{
					var range = NumberSubstitutionRanges[numberSubstitutionIndex];
					if (textIndex < range.TextPosition)
					{
						// Before start of range
						if (spanEnd > range.TextPosition)
							spanEnd = range.TextPosition;
					}
					else
					{
						// Inside range
						if (spanEnd > range.TextEnd)
							spanEnd = range.TextEnd;

						numberSubstitution = range.Value;
					}
				}
			}

			if (textIndex >= IsDigitRanges[isDigitIndex].TextEnd)
			{
				isDigitIndex++;
			}

			if (spanEnd > IsDigitRanges[isDigitIndex].TextEnd)
				spanEnd = IsDigitRanges[isDigitIndex].TextEnd;

			var isDigit = IsDigitRanges[isDigitIndex].Value;

			bool hasCombiningMark = false;
			for (int i = textIndex; i < spanEnd; i++)
			{
				if ((pCharAttribute[i] & CharAttribute.IsCombining) != 0)
				{
					hasCombiningMark = true;
					break;
				}
			}

			bool needsCaretInfo = true;
			for (int i = textIndex; i < spanEnd; i++)
			{
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
            for (int i = textIndex; i < spanEnd; ++i)
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
                    scriptAnalysis,
                    numberSubstitution,
                    isDigit ? numberCulture : null,
                    hasCombiningMark,
                    needsCaretInfo,
                    hasExtended,
                    isIndic,
                    isLatin
                    );

			result.Add(new Span(itemProps, spanEnd - textIndex));

			textIndex = spanEnd;
		}

		return result;
	}

    internal void SetIsDigit(
                                 uint textPosition,
                                 uint textLength,
                                 bool   isDigit
                                 )
    {
		IsDigitRanges.Add(new TextAnalysisRange<bool>((int)textPosition, (int)textLength, isDigit));
    }
}
}
