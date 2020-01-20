using System;
using System.Collections.Generic;

namespace MS.Internal.Text.TextInterface
{
	internal struct TextAnalysisRange<T>
	{
		internal int TextPosition;
		internal int TextLength;
		internal T Value;

		internal int TextEnd
		{
			get
			{
				return TextPosition + TextLength;
			}
		}

		internal TextAnalysisRange(int position, int length, T val)
		{
			TextPosition = position;
			TextLength = length;
			Value = val;
		}
	}

	internal class TextAnalyzerSink : IDWriteTextAnalysisSink
	{
		internal List<TextAnalysisRange<IDWriteNumberSubstitution>> NumberSubstitution = new List<TextAnalysisRange<IDWriteNumberSubstitution>>();
		internal List<TextAnalysisRange<DWriteScriptAnalysis>> ScriptAnalysis = new List<TextAnalysisRange<DWriteScriptAnalysis>>();

		public void SetScriptAnalysis(uint position, uint length, ref DWriteScriptAnalysis scriptanalysis)
		{
			ScriptAnalysis.Add(new TextAnalysisRange<DWriteScriptAnalysis>((int)position, (int)length, scriptanalysis));
		}

		public void SetLineBreakpoints(uint position, uint length,	ref byte breakpoints)
		{
		}

		public void SetBidiLevel(uint position, uint length, byte explicitLevel, byte resolvedLevel)
		{
		}

		public void SetNumberSubstitution(uint position, uint length, IDWriteNumberSubstitution substitution)
		{
			NumberSubstitution.Add(new TextAnalysisRange<IDWriteNumberSubstitution>((int)position, (int)length, substitution));
		}
	}
}
