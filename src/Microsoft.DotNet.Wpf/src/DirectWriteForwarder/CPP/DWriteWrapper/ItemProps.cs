// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;

namespace MS.Internal.Text.TextInterface
{
	public sealed class ItemProps
	{
		public DWriteScriptAnalysis? ScriptAnalysis {
			get; private set;
		}

		public IDWriteNumberSubstitution NumberSubstitution {
			get; private set;
		}

		public CultureInfo DigitCulture {
			get; private set;
		}

		public bool HasExtendedCharacter {
			get; private set;
		}

		public bool NeedsCaretInfo {
			get; private set;
		}

		public bool HasCombiningMark {
			get; private set;
		}

		public bool IsIndic {
			get; private set;
		}

		public bool IsLatin {
			get; private set;
		}

		public ItemProps()
		{
		}

		public ItemProps(
			DWriteScriptAnalysis? scriptAnalysis,
			IDWriteNumberSubstitution numberSubstitution,
			CultureInfo digitCulture,
			bool hasCombiningMark,
			bool needsCaretInfo,
			bool hasExtendedCharacter,
			bool isIndic,
			bool isLatin
			)
		{
			ItemProps result = new ItemProps();

			DigitCulture          = digitCulture;
			HasCombiningMark      = hasCombiningMark;
			HasExtendedCharacter  = hasExtendedCharacter;
			NeedsCaretInfo        = needsCaretInfo;
			IsIndic               = isIndic;
			IsLatin               = isLatin;

			ScriptAnalysis = scriptAnalysis;
			NumberSubstitution = numberSubstitution;
		}

		public bool CanShapeTogether(ItemProps other)
		{
			// Check whether 2 ItemProps have the same attributes that impact shaping so
			// it is possible to shape them together.
			bool canShapeTogether =  (NumberSubstitution == other.NumberSubstitution // They must have the same number substitution properties.
				&& 
				ScriptAnalysis.Equals(other.ScriptAnalysis));

			return canShapeTogether;
		}
	}
}
