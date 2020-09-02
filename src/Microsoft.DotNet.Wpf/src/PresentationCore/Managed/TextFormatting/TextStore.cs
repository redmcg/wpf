// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//
//
//  Contents:  FullTextLine text store
//
//


using System;
using System.Text;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using MS.Internal.Shaping;
using MS.Internal.Generic;
using System.Security;
using SR=MS.Internal.PresentationCore.SR;
using SRID=MS.Internal.PresentationCore.SRID;
using MS.Internal;
using MS.Internal.FontCache;
using MS.Internal.Text.TextInterface;
using Common.TextFormatting;

namespace Managed.TextFormatting
{
    internal class TextStore
    {
        private FormatSettings          _settings;                  // format settings
        private int                     _lscpFirstValue;            // first lscp value
        private int                     _cpFirst;                   // store first cp (both cp and lscp start out the same)
        private int                     _lscchUpTo;                 // number of lscp resolved
        private int                     _cchUpTo;                   // number of cp resolved
        private int                     _cchEol;                    // number of chars for end-of-line mark
        private int                     _accNominalWidthSoFar;      // accumulated nominal width so far
        private int                     _accTextLengthSoFar;        // accumulated count of text characters so far
        private NumberContext           _numberContext;             // cached number context for contextual digit substitution
        private int                     _cpNumberContext;           // cp at which _numberContext is valid

        private SpanVector              _plsrunVector;
        private SpanPosition            _plsrunVectorLatestPosition;
        private ArrayList               _lsrunList;                 // lsrun list
        private BidiState               _bidiState;                 // (defer initialization until FetchRun)
        private TextModifierScope       _modifierScope;             // top-most frame of the text modifier stack, or null

        private int                     _formatWidth;               // formatting width LS sees
        private SpanVector              _textObjectMetricsVector;   // inline object cache




        /// <summary>
        /// Constructing an intermediate text store for FullTextLine
        /// </summary>
        /// <param name="settings">text formatting settings</param>
        /// <param name="cpFirst">first cp of the line</param>
        /// <param name="lscpFirstValue">lscp first value</param>
        /// <param name="formatWidth">formatting width LS sees</param>
        public TextStore(
            FormatSettings          settings,
            int                     cpFirst,
            int                     lscpFirstValue,
            int                     formatWidth
            )
        {
            _settings = settings;
            _formatWidth = formatWidth;

            _cpFirst = cpFirst;
            _lscpFirstValue = lscpFirstValue;

            _lsrunList = new ArrayList(2);
            _plsrunVector = new SpanVector(null);
            _plsrunVectorLatestPosition = new SpanPosition();

            // Recreate the stack of text modifiers if there is one.
            TextLineBreak previousLineBreak = settings.PreviousLineBreak;

            if (    previousLineBreak != null
                &&  previousLineBreak.TextModifierScope != null)
            {
                _modifierScope = previousLineBreak.TextModifierScope.CloneStack();

                // Construct bidi state from input settings and modifier scopes
                _bidiState = new BidiState(_settings, _cpFirst, _modifierScope);
            }
        }


        /// <summary>
        /// Wrapper to TextRun fetching from the cache
        /// </summary>
        internal TextRunInfo  FetchTextRun(int cpFetch)
        {
            int runLength;
            TextRun textRun;

            // fetch TextRun from the formatting state
            CharacterBufferRange charString = _settings.FetchTextRun(
                cpFetch,
                _cpFirst,
                out textRun,
                out runLength
                );

            CultureInfo digitCulture = null;
            bool contextualSubstitution = false;
            bool symbolTypeface = false;

            Plsrun runType = TextRunInfo.GetRunType(textRun);

            if (runType == Plsrun.Text)
            {
                TextRunProperties properties = textRun.Properties;
                symbolTypeface = properties.Typeface.Symbol;
                if (!symbolTypeface)
                {
                    _settings.DigitState.SetTextRunProperties(properties);
                    digitCulture = _settings.DigitState.DigitCulture;
                    contextualSubstitution = _settings.DigitState.Contextual;
                }
            }

            TextModifierScope currentScope = _modifierScope;
            TextModifier modifier = textRun as TextModifier;

            if (modifier != null)
            {
                _modifierScope = new TextModifierScope(
                    _modifierScope,
                    modifier,
                    cpFetch
                    );

                // The new scope inclues the current TextModifier run
                currentScope = _modifierScope;
            }
            else if (_modifierScope != null && textRun is TextEndOfSegment)
            {
                // The new scope only affects subsequent runs. TextEndOfSegment run itself is
                // still in the old scope such that its coresponding TextModifier run can be tracked.
                _modifierScope = _modifierScope.ParentScope;
            }

            return new TextRunInfo(
                charString,
                runLength,
                cpFetch - _cpFirst, // offsetToFirstCp
                textRun,
                runType,
                0,   // charFlags
                digitCulture,
                contextualSubstitution,
                symbolTypeface,
                currentScope
                );
        }


		public LineBreakpoints FindLineBreakpoints(int rangeStart, int rangeLength)
		{
			var ranges = new List<CharacterBufferRange>();
			var lengths = new List<uint>();

			// This is probably slow, but in theory dwrite may need the full line to figure out linebreaks.
			int lineStart = _cpFirst;
			int pos = lineStart;
			while (true)
			{
				int runLength;
				TextRun textRun;

				CharacterBufferRange charString = _settings.FetchTextRun(
					pos,
					_cpFirst,
					out textRun,
					out runLength
					);

				if (runLength == 0)
					break;

				if (textRun is TextEndOfLine)
				{
					if (pos < lineStart)
					{
						// Previous line in this buffer
						pos += runLength;
						lineStart = pos;
						ranges.Clear();
						lengths.Clear();
						continue;
					}
					else
					{
						// End of current line
						break;
					}
				}

				ranges.Add(charString);
				lengths.Add((uint)runLength);
				pos += runLength;
			}

			// get raw pointers to text
			IntPtr[] textPtrs = new IntPtr[ranges.Count];
			List<GCHandle> pinHandles = new List<GCHandle>(ranges.Count);
			LineBreakpoints result;

			try
			{
				for (int i=0; i<ranges.Count; i++)
				{
					GCHandle pinHandle;
					textPtrs[i] = ranges[i].CharacterBuffer.PinAndGetCharacterPointer(
						ranges[i].OffsetToFirstChar, out pinHandle);
					pinHandles.Add(pinHandle);
				}

				var textAnalyzer = _settings.Formatter.TextAnalyzer;

				result = textAnalyzer.AnalyzeLineBreakpoints(
					textPtrs,
					lengths.ToArray(),
					_settings.Pap.DefaultCultureInfo,
					DWriteFactory.Instance,
					(uint)(rangeStart - lineStart),
					(uint)rangeLength,
					_settings.Pap.RightToLeft, //isRightToLeft
					CultureInfo.CurrentCulture, // dummy numberSubstitution values, this probably doesn't matter for linebreaks?
					false,
					(uint)NumberSubstitutionMethod.Context);

				result.RangeStart += lineStart;
			}
			finally
			{
				for (int i=0; i<pinHandles.Count; i++)
				{
					ranges[i].CharacterBuffer.UnpinCharacterPointer(pinHandles[i]);
				}
			}

			//FIXME: Handle inline objects and custom hyphenators

			return result;
		}


        /// <summary>
        /// Split a TextRunInfo into multiple ranges each with a uniform set of
        /// TextEffects.
        /// </summary>
        /// <remarks>
        /// A TextRun can have a collection of TextEffect. Each of them can be applied to
        /// an arbitrary range of text. This method breaks the TextRunInfo into sub-ranges
        /// that have identical set of TextEffects. For example
        ///
        /// Current Run :   |----------------------------------------|
        /// Effect 1:     |------------------------------------------------------|
        /// Effect 2:                  |------------------------|
        /// Splitted runs:  |----------|------------------------|----|
        ///
        /// It can be observed that the effected ranges are dividied at the boundaries of the
        /// TextEffects. We sort all the boundaries of TextEffects according to their positions
        /// and create the effected range in between of any two ajacent boundaries. For each efffected
        /// range, we store all the active TextEffect into a list.
        /// </remarks>
        private void SetTextEffectsVector(
            SpanVector              textEffectsVector,
            int                     ich,
            TextRunInfo             runInfo,
            TextEffectCollection    textEffects
            )
        {
            // We already check for empty text effects at the call site.
            Debug.Assert(textEffects != null && textEffects.Count != 0);

            int cpFetched = _cpFirst + _cchUpTo + ich; // get text source character index

            // Offset from client Cp to text effect index.
            int offset = cpFetched - _settings.TextSource.GetTextEffectCharacterIndexFromTextSourceCharacterIndex(cpFetched);

            int textEffectsCount = textEffects.Count;
            TextEffectBoundary[] bounds = new TextEffectBoundary[textEffectsCount * 2];
            for (int i = 0; i < textEffectsCount; i++)
            {
                TextEffect effect = textEffects[i];
                bounds[2 * i] = new TextEffectBoundary(effect.PositionStart, true); // effect starting boundary
                bounds[2 * i + 1] = new TextEffectBoundary(effect.PositionStart + effect.PositionCount, false); // effect end boundary
            }

            Array.Sort(bounds); // sort the TextEffect bounds.

            int effectedRangeStart = Math.Max(cpFetched - offset, bounds[0].Position);
            int effectedRangeEnd   = Math.Min(cpFetched - offset + runInfo.Length, bounds[bounds.Length - 1].Position);

            int currentEffectsCount = 0;
            int currentPosition = effectedRangeStart;
            for (int i = 0; i < bounds.Length && currentPosition < effectedRangeEnd; i++)
            {
                // Have we reached the end of a non-empty subrange with at least one text effect?
                if (currentPosition < bounds[i].Position && currentEffectsCount > 0)
                {
                    // Let [currentPosition,currentRangeEnd) delimit the subrange ending at bounds[i].
                    int currentRangeEnd = Math.Min(bounds[i].Position, effectedRangeEnd);

                    // Consolidate all the active effects in the subrange.
                    IList<TextEffect> activeEffects = new TextEffect[currentEffectsCount];
                    int effectIndex = 0;
                    for (int j = 0; j < textEffectsCount; j++)
                    {
                        TextEffect effect = textEffects[j];
                        if (currentPosition >= effect.PositionStart && currentPosition < (effect.PositionStart + effect.PositionCount))
                        {
                            activeEffects[effectIndex++] = effect;
                        }
                    }

                    Invariant.Assert(effectIndex == currentEffectsCount);

                    // Set the active effects for this CP subrange. The vector index is relative
                    // to the starting cp of the current run-fetching loop.
                    textEffectsVector.SetReference(
                        currentPosition + offset - _cchUpTo - _cpFirst,    // client cp index
                        currentRangeEnd - currentPosition,                 // length
                        activeEffects                                      // text effects
                        );

                    currentPosition = currentRangeEnd;
                }

                // Adjust the current count depending on if it is a TextEffect's starting or ending boundary.
                currentEffectsCount += (bounds[i].IsStart ? 1 : -1);

                if (currentEffectsCount == 0 && i < bounds.Length - 1)
                {
                   // There is no effect on the current position. Move it to the start of next TextEffect.
                   Invariant.Assert(bounds[i + 1].IsStart);
                   currentPosition = Math.Max(currentPosition, bounds[i + 1].Position);
                }
            }
        }

        /// <summary>
        /// Structure representing one boundary of a TextEffect. Each TextEffect has
        /// two boundaries: the beginning and the end.
        /// </summary>
        private struct TextEffectBoundary : IComparable<TextEffectBoundary>
        {
            private readonly int _position;
            private readonly bool _isStart;

            internal TextEffectBoundary(int position, bool isStart)
            {
                _position = position;
                _isStart = isStart;
            }

            internal int Position
            {
                get { return _position; }
            }

            internal bool IsStart
            {
                get { return _isStart; }
            }

            public int CompareTo(TextEffectBoundary other)
            {
                if (Position != other.Position)
                    return Position - other.Position;

                if (IsStart == other.IsStart) return 0;

                // Starting edge is always in front.
                return IsStart ? -1 : 1;
            }
        }


        /// <summary>
        /// Create special run that matches the content of specified text run
        /// </summary>
        private TextRunInfo CreateSpecialRunFromTextContent(
            TextRunInfo     runInfo,
            int             cchFetched
            )
        {
            // -FORMAT ANCHOR-
            //
            // Format anchor character is what we create internally to drive LS. If it
            // is present in the middle of text stream sent from the client, we will
            // have to filter it out and replace it with NBSP. This is to protect LS
            // from running into a bad state due to misinterpreting such character as
            // our format anchor. Following is the list of anchor character we use todate.
            //
            //      "\uFFFB" (Unicode 'Annotation Terminator')
            //
            // -LINEBREAK-
            //
            // Following the Unicode guideline on newline characters, we recognize
            // both LS (U+2028) and PS (U+2029) as explicit linebreak (PS also breaks
            // paragraph but that's handled outside line level formatting). We also
            // treat the following sequence of characters as linebreak
            //
            //      "CR"    ("\u000D")
            //      "LF"    ("\u000A")
            //      "CRLF"  ("\u000D\u000A")
            //      "NEL"   ("\u0085")
            //      "VT"    ("\u000B")
            //      "FF"    ("\u000C")
            //
            // Note: http://www.unicode.org/unicode/reports/tr13/tr13-9.html
            Debug.Assert(runInfo.StringLength > 0 && runInfo.Length > 0);

            CharacterBuffer charBuffer = runInfo.CharacterBuffer;
            int offsetToFirstChar = runInfo.OffsetToFirstChar;
            char firstChar = charBuffer[offsetToFirstChar];
            ushort charFlags;

            charFlags = (ushort)Classification.CharAttributeOf(
                (int)Classification.GetUnicodeClassUTF16(firstChar)
                ).Flags;

            if ((charFlags & (ushort)CharacterAttributeFlags.CharacterLineBreak) != 0)
            {
                // Get cp length of newline sequence
                //
                // It is possible that client run ends in between two codepoints that
                // make up a single newline sequence e.g. CRLF. Therefore, when we
                // encounter the first codepoint of the sequence, we need to make sure
                // we have enough codepoints to determine the correct whole sequence.
                // In an uncommon event, we may be forced to look ahead by fetching more
                // runs.

                int newlineLength = 1;  // most sequences take one cp

                if (firstChar == '\r')
                {
                    if (runInfo.Length > 1)
                    {
                        newlineLength += ((charBuffer[offsetToFirstChar + 1] == '\n') ? 1 : 0);
                    }
                    else
                    {
                        TextRunInfo nextRunInfo = FetchTextRun(_cpFirst + cchFetched + 1);

                        if (nextRunInfo != null && nextRunInfo.TextRun is ITextSymbols)
                        {
                            newlineLength += ((nextRunInfo.CharacterBuffer[nextRunInfo.OffsetToFirstChar] == '\n') ? 1 : 0);
                        }
                    }
                }

                unsafe
                {
                    runInfo = new TextRunInfo(
                        new CharacterBufferRange(StrLineSeparator, 0, 1),
                        newlineLength, // run length
                        runInfo.OffsetToFirstCp,
                        runInfo.TextRun,
                        Plsrun.LineBreak, // LineBreak run
                        charFlags,
                        null,  // digit culture
                        false, // contextual substitution
                        false, // is not Unicode
                        runInfo.TextModifierScope
                        );
                }
            }
            else if ((charFlags & (ushort)CharacterAttributeFlags.CharacterParaBreak) != 0)
            {
                unsafe
                {
                    // This character is a paragraph separator. Split it into a
                    // separate run.
                    runInfo = new TextRunInfo(
                        new CharacterBufferRange(StrParaSeparator, 0, 1),
                        1,
                        runInfo.OffsetToFirstCp,
                        runInfo.TextRun,
                        Plsrun.ParaBreak,  // parabreak run
                        charFlags,
                        null,   // digit culture
                        false,  // contextual substitution
                        false,  // is not Unicode
                        runInfo.TextModifierScope
                        );
                }
            }
            else
            {
                Invariant.Assert((charFlags & (ushort)CharacterAttributeFlags.CharacterFormatAnchor) != 0);

                unsafe
                {
                    runInfo = new TextRunInfo(
                        new CharacterBufferRange(StrNbsp, 0, 1),
                        1, // run length
                        runInfo.OffsetToFirstCp,
                        runInfo.TextRun,
                        runInfo.Plsrun,
                        charFlags,
                        null,   // digit culture
                        false,  // contextual substitution
                        false,  // is not Unicode
                        runInfo.TextModifierScope
                        );
                }
            }

            return runInfo;
        }


        /// <summary>
        /// Get the Bidi level of the character before the currently fetched one
        /// </summary>
        private int GetLastLevel()
        {
			throw new NotImplementedException("Managed.TextFormatting.TextStore.GetLastLevel");
        }


        /// <summary>
        /// Base bidi level
        /// </summary>
        private int BaseBidiLevel
        {
            get { return _settings.Pap.RightToLeft ? 1 : 0; }
        }

        /// <summary>
        /// Analyze bidirectional level of runs
        /// </summary>
        /// <param name="runInfoVector">run info vector indexed by ich</param>
        /// <param name="stringLength">character length of string to be analyzed</param>
        /// <param name="bidiLevels">array of bidi levels, each for a character</param>
        /// <returns>Number of characters resolved</returns>
        /// <remarks>
        /// BiDi Analysis in line layout imposes a higher level protocol on top of Unicode bidi algorithm
        /// to support rich editing behavior. Explicit directional embedding controls is to be done
        /// through TextModifier runs and corresponding TextEndOfSegment. Directional controls (such as
        /// LRE, RLE, PDF, etc) in the text stream are ignored in the Bidi Analysis to avoid conflict with the higher
        /// level protocol.
        ///
        /// The implementation analyzes directional embedding one level at a time. Input text runs are divided
        /// at the point where directional embedding level is changed.
        /// </remarks>
        private int BidiAnalyze(
            SpanVector                  runInfoVector,
            int                         stringLength,
            out byte[]                  bidiLevels
            )
        {
            CharacterBuffer charBuffer = null;
            int offsetToFirstChar;

            SpanRider runInfoSpanRider = new SpanRider(runInfoVector);
            if (runInfoSpanRider.Length >= stringLength)
            {
                // typical case, only one string is analyzed
                TextRunInfo runInfo = (TextRunInfo)runInfoSpanRider.CurrentElement;

                if (!runInfo.IsSymbol)
                {
                    charBuffer = runInfo.CharacterBuffer;
                    offsetToFirstChar = runInfo.OffsetToFirstChar;
                    Debug.Assert(runInfo.StringLength >= stringLength);
                }
                else
                {
                    // Treat all characters in non-Unicode runs as strong left-to-right.
                    // The literal 'A' could be any Latin character.
                    charBuffer = new StringCharacterBuffer(new string('A', stringLength));
                    offsetToFirstChar = 0;
                }
            }
            else
            {
                // build up a consolidated character buffer for bidi analysis of
                // concatenated strings in multiple textruns.
                int ich = 0;
                int cch;

                StringBuilder stringBuilder = new StringBuilder(stringLength);

                while(ich < stringLength)
                {
                    runInfoSpanRider.At(ich);
                    cch = runInfoSpanRider.Length;
                    TextRunInfo runInfo = (TextRunInfo)runInfoSpanRider.CurrentElement;

                    Debug.Assert(cch <= runInfo.StringLength);

                    if (!runInfo.IsSymbol)
                    {
                        runInfo.CharacterBuffer.AppendToStringBuilder(
                            stringBuilder,
                            runInfo.OffsetToFirstChar,
                            cch
                            );
                    }
                    else
                    {
                        // Treat all characters in non-Unicode runs as strong left-to-right.
                        // The literal 'A' could be any Latin character.
                        stringBuilder.Append('A', cch);
                    }

                    ich += cch;
                }

                charBuffer = new StringCharacterBuffer(stringBuilder.ToString());
                offsetToFirstChar = 0;
            }

            if(_bidiState == null)
            {
                // make sure the initial state is setup
                _bidiState = new BidiState(_settings, _cpFirst);
            }

            bidiLevels = new byte[stringLength];
            DirectionClass[] directionClasses = new DirectionClass[stringLength];

            int resolvedLength = 0;

            for(int i = 0; i < runInfoVector.Count; i++)
            {
                int cchResolved = 0;

                TextRunInfo currentRunInfo = (TextRunInfo) runInfoVector[i].element;
                TextModifier modifier = currentRunInfo.TextRun as TextModifier;

                if (IsDirectionalModifier(modifier))
                {
                    bidiLevels[resolvedLength] = AnalyzeDirectionalModifier(_bidiState, modifier.FlowDirection);
                    cchResolved = 1;
                }
                else if (IsEndOfDirectionalModifier(currentRunInfo))
                {
                    bidiLevels[resolvedLength] = AnalyzeEndOfDirectionalModifier(_bidiState);
                    cchResolved = 1;
                }
                else
                {
                    int ich = resolvedLength;
                    do
                    {
                        CultureInfo culture = CultureMapper.GetSpecificCulture(currentRunInfo.Properties == null ? null : currentRunInfo.Properties.CultureInfo);
                        DirectionClass europeanNumberOverride = _bidiState.GetEuropeanNumberClassOverride(culture);

                        //
                        // The European number in the input text is explictly set to AN or EN base on the
                        // culture of the text. We set the input DirectionClass of this range of text to
                        // AN or EN to indicate that any EN in this range should be explicitly set to this override
                        // value.
                        //
                        for(int k = 0; k < runInfoVector[i].length; k++)
                        {
                            directionClasses[ich + k] = europeanNumberOverride;
                        }

                        ich += runInfoVector[i].length;
                        if ((++i) >= runInfoVector.Count)
                            break; // end of all runs.

                        currentRunInfo = (TextRunInfo) runInfoVector[i].element;
                        if ( currentRunInfo.Plsrun == Plsrun.Hidden &&
                              (  IsDirectionalModifier(currentRunInfo.TextRun as TextModifier)
                              || IsEndOfDirectionalModifier(currentRunInfo)
                              )
                           )
                        {
                            i--;
                            break;   // break bidi analysis at the point of embedding level change
                        }
                    }
                    while (true);

                    const Bidi.Flags BidiFlags = Bidi.Flags.ContinueAnalysis | Bidi.Flags.IgnoreDirectionalControls | Bidi.Flags.OverrideEuropeanNumberResolution;

                    // The last runs will be marked as IncompleteText as their resolution
                    // may depend on following runs that haven't been fetched yet.
                    Bidi.Flags flags = (i < runInfoVector.Count) ?
                            BidiFlags
                          : BidiFlags | Bidi.Flags.IncompleteText;


                    Bidi.BidiAnalyzeInternal(
                        charBuffer,
                        offsetToFirstChar + resolvedLength,
                        ich - resolvedLength,
                        0, // no max hint
                        flags,
                        _bidiState,
                        new PartialArray<byte>(bidiLevels, resolvedLength, ich - resolvedLength),
                        new PartialArray<DirectionClass>(directionClasses, resolvedLength, ich - resolvedLength),
                        out cchResolved
                        );

                    // Text must be completely resolved if there is no IncompleteText flag.
                    Invariant.Assert(cchResolved == ich - resolvedLength || (flags & Bidi.Flags.IncompleteText) != 0);
                }

                resolvedLength += cchResolved;
            }

            Invariant.Assert(resolvedLength <= bidiLevels.Length);
            return resolvedLength;
        }

        /// <summary>
        /// Update BidiState base to the new directional embedding level.
        /// </summary>
        /// <returns>
        /// The method returns the embedding level before the start of the Modifier.
        /// Contents inside the modifier scope is at a higher embedding level and hence
        /// separated from the content before the modifier scope.
        /// </returns>
        private byte AnalyzeDirectionalModifier(
            BidiState       state,
            FlowDirection   flowDirection
            )
        {
            bool leftToRight = (flowDirection == FlowDirection.LeftToRight);

            ulong levelStack = state.LevelStack;

            byte parentLevel = Bidi.BidiStack.GetMaximumLevel(levelStack);

            byte topLevel;

            // Push to Bidi stack. Increment overflow counter if so.
            if (!Bidi.BidiStack.Push(ref levelStack, leftToRight, out topLevel))
            {
                state.Overflow++;
            }

            state.LevelStack = levelStack;

            // set the default last strong such that text without CultureInfo
            // can be resolved correctly.
            state.SetLastDirectionClassesAtLevelChange();
            return parentLevel;
        }

        /// <summary>
        /// Update BidiState at the end of a directional emebedding level.
        /// </summary>
        /// <returns>
        /// The method returns the embedding level after the end of the modifier.
        /// Contents inside the modifier scope is at a higher embedding level and hence separated
        /// from the content after the modifier scope.
        /// </returns>
        private byte AnalyzeEndOfDirectionalModifier(BidiState state)
        {
            // Pop level stack
            if (state.Overflow > 0)
            {
                state.Overflow --;
                return state.CurrentLevel;
            }

            byte parentLevel;
            ulong stack = state.LevelStack;

            bool success = Bidi.BidiStack.Pop(ref stack, out parentLevel);
            Invariant.Assert(success);
            state.LevelStack = stack;

            // set the default last strong such that text without CultureInfo
            // can be resolved correctly.
            state.SetLastDirectionClassesAtLevelChange();
            return parentLevel;
        }

        private bool IsEndOfDirectionalModifier(TextRunInfo runInfo)
        {
            return (  runInfo.TextModifierScope != null
                   && runInfo.TextModifierScope.TextModifier.HasDirectionalEmbedding
                   && runInfo.TextRun is TextEndOfSegment
                   );
        }

        private bool IsDirectionalModifier(TextModifier modifier)
        {
            return modifier != null && modifier.HasDirectionalEmbedding;
        }

        /// <summary>
        /// Determines whether a line needs to be truncated for security reasons due to exceeding
        /// the maximum number of characters per line. See the comment for MaxCharactersPerLine.
        /// </summary>
        /// <param name="runInfoVector">Vector of fetched text runs.</param>
        /// <param name="cchToAdd">Number of cp to be added to _plsrunVector; the method
        /// may change this value if the line needs to be truncated.</param>
        /// <returns>Returns true if the line should be truncated, false it not.</returns>
        private bool IsForceBreakRequired(SpanVector runInfoVector, ref int cchToAdd)
        {
            bool forceBreak = false;
            int ichRun = 0;

            for (int i = 0; i < runInfoVector.Count && ichRun < cchToAdd; ++i)
            {
                Span span = runInfoVector[i];
                TextRunInfo runInfo = (TextRunInfo)span.element;

                int runLength = Math.Min(span.length, cchToAdd - ichRun);

                // Only Plsrun.Text runs count against the limit
                if (runInfo.Plsrun == Plsrun.Text && !IsNewline((ushort)runInfo.CharacterAttributeFlags))
                {
                    if (_accTextLengthSoFar + runLength <= MaxCharactersPerLine)
                    {
                        // we're still under the limit; accumulate the number of characters so far
                        _accTextLengthSoFar += runLength;
                    }
                    else
                    {
                        // accumulated number of characters has exceeded the maximum allowed number
                        // of characters per line; we need to generate a fake line break
                        runLength = MaxCharactersPerLine - _accTextLengthSoFar;
                        _accTextLengthSoFar = MaxCharactersPerLine;
                        cchToAdd = ichRun + runLength;
                        forceBreak = true;
                    }
                }

                ichRun += runLength;
            }

            return forceBreak;
        }

        [Flags]
        private enum NumberContext
        {
            Unknown             = 0,

            Arabic              = 0x0001,
            European            = 0x0002,
            Mask                = 0x0003,

            FromLetter          = 0x0004,
            FromFlowDirection   = 0x0008
        }

        private NumberContext GetNumberContext(TextModifierScope scope)
        {
            int limitCp = _cpFirst + _cchUpTo;
            int firstCp = _cpNumberContext;
            NumberContext cachedNumberContext = _numberContext;

            // Is there a current bidi scope?
            for (; scope != null; scope = scope.ParentScope)
            {
                if (scope.TextModifier.HasDirectionalEmbedding)
                {
                    int cpScope = scope.TextSourceCharacterIndex;
                    if (cpScope >= _cpNumberContext)
                    {
                        // Only scan back to the start of the current scope and don't use the cached number
                        // context since it's outside the current scope.
                        firstCp = cpScope;
                        cachedNumberContext = NumberContext.Unknown;
                    }
                    break;
                }
            }

            // Is it a right to left context?
            bool rightToLeft = (scope != null) ?
                scope.TextModifier.FlowDirection == FlowDirection.RightToLeft :
                Pap.RightToLeft;

            // Scan for a preceding letter.
            while (limitCp > firstCp)
            {
                TextSpan<CultureSpecificCharacterBufferRange> textSpan = _settings.GetPrecedingText(limitCp);

                // Stop if there's an empty TextSpan
                if (textSpan.Length <= 0)
                {
                    break;
                }

                CharacterBufferRange charRange = textSpan.Value.CharacterBufferRange;
                if (!charRange.IsEmpty)
                {
                    CharacterBuffer charBuffer = charRange.CharacterBuffer;

                    // Index just past the last character in the range.
                    int limit = charRange.OffsetToFirstChar + charRange.Length;

                    // Index of the first character in the range, not including any characters before firstCp.
                    int first = limit - Math.Min(charRange.Length, limitCp - firstCp);

                    // We'll stop scanning at letter or line break.
                    const ushort flagsMask =
                        (ushort)CharacterAttributeFlags.CharacterLetter |
                        (ushort)CharacterAttributeFlags.CharacterLineBreak;

                    // Iterate over the characters in reverse order.
                    for (int i = limit - 1; i >= first; --i)
                    {
                        char ch = charBuffer[i];
                        CharacterAttribute charAttributes = Classification.CharAttributeOf(Classification.GetUnicodeClassUTF16(ch));

                        ushort flags = (ushort)(charAttributes.Flags & flagsMask);
                        if (flags != 0)
                        {
                            if ((flags & (ushort)CharacterAttributeFlags.CharacterLetter) != 0)
                            {
                                // It's a letter so the number context depends on its script.
                                return (charAttributes.Script == (byte)ScriptID.Arabic || charAttributes.Script == (byte)ScriptID.Syriac) ?
                                    NumberContext.Arabic | NumberContext.FromLetter :
                                    NumberContext.European | NumberContext.FromLetter;
                            }
                            else
                            {
                                // It's a line break. There are no preceding letters so number context depends only on
                                // whether the current bidi scope is right to left.
                                return rightToLeft ?
                                    NumberContext.Arabic | NumberContext.FromFlowDirection :
                                    NumberContext.European | NumberContext.FromFlowDirection;
                            }
                        }
                    }
                }

                limitCp -= textSpan.Length;
            }

            // If we have a cached number context that's still valid the use it. Valid means (1) we
            // scanned back as far as the cp of the number context, and (2) the number context was
            // determined from a letter. (A cached number context derived from flow direction might
            // not be valid because an embedded bidi level may have ended.)
            if (limitCp <= firstCp && (cachedNumberContext & NumberContext.FromLetter) != 0)
            {
                return cachedNumberContext;
            }

            // There are no preceding letters so number context depends only on whether the current
            // bidi scope is right to left.
            return rightToLeft ?
                NumberContext.Arabic | NumberContext.FromFlowDirection :
                NumberContext.European | NumberContext.FromFlowDirection;
        }

        #region lsrun/cp mapping

        /// <summary>
        /// Map internal LSCP to text source cp
        /// </summary>
        /// <remarks>
        /// This method does not handle mapping of LSCP beyond the last one
        /// </remarks>
        internal int GetExternalCp(int lscp)
        {
			throw new NotImplementedException("Managed.TextFormatting.TextStore.GetExternalCp");
        }


        /// <summary>
        /// Check if plsrun is marker
        /// </summary>
        internal static bool IsMarker(Plsrun plsrun)
        {
            return (plsrun & Plsrun.IsMarker) != 0;
        }


        /// <summary>
        /// Make this plsrun a marker plsrun
        /// </summary>
        internal static Plsrun MakePlsrunMarker(Plsrun plsrun)
        {
            return (plsrun | Plsrun.IsMarker);
        }


        /// <summary>
        /// Make this plsrun a symbol plsrun
        /// </summary>
        internal static Plsrun MakePlsrunSymbol(Plsrun plsrun)
        {
            return (plsrun | Plsrun.IsSymbol);
        }


        /// <summary>
        /// Convert plsrun to index to lsrun list
        /// </summary>
        internal static Plsrun ToIndex(Plsrun plsrun)
        {
            return (plsrun & Plsrun.UnmaskAll);
        }


        /// <summary>
        /// Check if run is content
        /// </summary>
        internal static bool IsContent(Plsrun plsrun)
        {
            plsrun = ToIndex(plsrun);
            return plsrun >= Plsrun.FormatAnchor;
        }


        /// <summary>
        /// Check if character is space
        /// </summary>
        internal static bool IsSpace(char ch)
        {
            return ch == ' ' || ch == '\u00a0';
        }


        /// <summary>
        /// Check if character is of strong directional type
        /// </summary>
        internal static bool IsStrong(char ch)
        {
            int unicodeClass = Classification.GetUnicodeClass(ch);
            ItemClass itemClass = (ItemClass)Classification.CharAttributeOf(unicodeClass).ItemClass;
            return itemClass == ItemClass.StrongClass;
        }


        /// <summary>
        /// Check if the run is a line break or paragraph break
        /// </summary>
        internal static bool IsNewline (Plsrun plsrun)
        {
            return plsrun == Plsrun.LineBreak || plsrun == Plsrun.ParaBreak;
        }

        /// <summary>
        /// Check if the character is a line break or paragraph break character
        /// </summary>
        internal static bool IsNewline(ushort flags)
        {
            return ( (flags & (ushort) CharacterAttributeFlags.CharacterLineBreak) != 0
                  || (flags & (ushort) CharacterAttributeFlags.CharacterParaBreak) != 0 );
        }

        #endregion


        /// <summary>
        /// Collect a piece of raw text that makes up a word containing the specified LSCP.
        /// The text returned from this method is used for hyphenation of a single word.
        /// In addition to the raw text, it also returns the mapping between the raw character
        /// indices and the LSCP indices in plsrunVector. This is used later on when we
        /// map the lexical result back to the positions used to communicate with LS.
        /// </summary>
        /// <remarks>
        /// "word" here is not meant for a linguistic term. It only means array of characters
        /// from space to space i.e. a word in SE Asian language is not separated by spaces.
        /// </remarks>
        internal char[] CollectRawWord(
            int                 lscpCurrent,
            bool                isCurrentAtWordStart,
            bool                isSideways,
            out int             lscpChunk,
            out int             lscchChunk,
            out CultureInfo     textCulture,
            out int             cchWordMax,
            out SpanVector<int> textVector
            )
        {
			throw new NotImplementedException("Managed.TextFormatting.TextStore.CollectRawWord");
        }


        /// <summary>
        /// Fetch cached inline metrics
        /// </summary>
        /// <param name="textObject">text object to format</param>
        /// <param name="cpFirst">firs cp of text object</param>
        /// <param name="currentPosition">inline's current pen position</param>
        /// <param name="rightMargin">line's right margin</param>
        /// <returns>inline info</returns>
        /// <remarks>
        /// Right margin is not necessarily the same as column max width. Right margin
        /// is usually greater than actual column width during object formatting. LS
        /// increases the margin to 1/32 of the column width to provide a leaway for
        /// breaking.
        ///
        /// However TextBlock/TextFlow functions in such a way that it needs to know the exact width
        /// left in the line in order to compute the inline's correct size. We make sure
        /// that it'll never get oversize max width.
        ///
        /// Inline object's reported size can be so huge that it may overflow LS's maximum value.
        /// If a given width is a finite value, we'll respect that and out-of-range exception may be thrown as appropriate.
        /// If the width is Positive Infinity, the width is trimmed to the maximum remaining value that LS can handle. This is
        /// appropriate for the cases where client measures inline objects at Infinite size.
        /// </remarks>
        internal TextEmbeddedObjectMetrics FormatTextObject(
            TextEmbeddedObject  textObject,
            int                 cpFirst,
            int                 currentPosition,
            int                 rightMargin
            )
        {
            if(_textObjectMetricsVector == null)
            {
                _textObjectMetricsVector = new SpanVector(null);
            }

            SpanRider rider = new SpanRider(_textObjectMetricsVector);
            rider.At(cpFirst);

            TextEmbeddedObjectMetrics metrics = (TextEmbeddedObjectMetrics)rider.CurrentElement;

            if(metrics == null)
            {
                int widthLeft = _formatWidth - currentPosition;

                if(widthLeft <= 0)
                {
                    // we're formatting this object outside the actual column width,
                    // we give the host the max width from the current position up
                    // to the margin.
                    widthLeft = rightMargin - _formatWidth;
                }

                metrics = textObject.Format(_settings.Formatter.IdealToReal(widthLeft, _settings.TextSource.PixelsPerDip));

                if (Double.IsPositiveInfinity(metrics.Width))
                {
                    // If the inline object has Width to be positive infinity, trim the width to
                    // the maximum value that LS can handle.
                    metrics = new TextEmbeddedObjectMetrics(
                        _settings.Formatter.IdealToReal((Constants.IdealInfiniteWidth - currentPosition), _settings.TextSource.PixelsPerDip),
                        metrics.Height,
                        metrics.Baseline
                        );
                }
                else if (metrics.Width > _settings.Formatter.IdealToReal((Constants.IdealInfiniteWidth - currentPosition), _settings.TextSource.PixelsPerDip))
                {
                    // LS cannot compute value greater than its maximum computable value
                    throw new ArgumentException(SR.Get(SRID.TextObjectMetrics_WidthOutOfRange));
                }

                _textObjectMetricsVector.SetReference(cpFirst, textObject.Length, metrics);
            }

            Debug.Assert(metrics != null);
            return metrics;
        }


        #region ENUMERATIONS & CONST

        // first negative cp of bullet marker
        internal const int LscpFirstMarker = (-0x7FFFFFFF);

        // Note: Trident uses this figure
        internal const int TypicalCharactersPerLine = 100;

        internal const char CharLineSeparator = '\x2028';
        internal const char CharParaSeparator = '\x2029';
        internal const char CharLineFeed      = '\x000a';
        internal const char CharCarriageReturn= '\x000d';
        internal const char CharTab           = '\x0009';
		internal const string StrObjectReplacement = "\xfffc";
		internal const string StrLineSeparator = "\x2028";
		internal const string StrParaSeparator = "\x2029";
		internal const string StrHidden = "\xf822"; // No idea what this "should" be, so using something arbitrary
		internal const string StrNbsp = "\x00A0";


        /// !! DO NOT update this enum without looking at its unmanaged pair in lslo.cpp !!
        /// [wchao, 10-1-2001]
        //
        internal enum ObjectId : ushort
        {
            Reverse         = 0,
            MaxNative       = 1,
            InlineObject    = 1,
            Max             = 2,
            Text_chp        = 0xffff,
        }

        // Maximum number of characters per line. If we exceed this number of characters
        // without breaking in a normal way, we chop off the line by generating a fake
        // line break run. This is to mitigate potential denial-of-service attacks by
        // ensuring that there is a reasonable upper bound on the time required to
        // format a single line.
        //
        // In ordinary documents with word-wrap enabled, we should always reach the
        // right margin long before MaxCharactersPerLine characters. The only reason
        // we might forcibly break the line in such cases would be:
        //   (a) Extreme right margin
        //   (b) Extreme number of zero-width characters
        //   (c) Extremely small font size (i.e., fraction of a pixel)
        //   (d) Lack of break opportunity (e.g., no spaces)
        // All of these are security cases, for which chopping off the line is a
        // reasonable mitigation. Limiting the line length addresses all of these
        // potential attacks so we don't need separate mitigations for, e.g.,
        // zero-width characters.
        //
        // Extremely long lines are less unlikely in nowrap scenarios, such as a code
        // editor. However, the same security issues apply so we still chop off the line
        // if we exceed the maximum number of characters with no line break. Note that
        // Notepad (with nowrap) does the same thing.
        //
        // The value chosen corresponds roughly to four pages of text at 60 characters
        // per line and 40 lines per page. Testing shows this to be a resonable limit
        // in terms of run time.
        internal const int MaxCharactersPerLine = 9600; // 60 * 40 * 4

        /// <summary>
        /// The maximum number of characters within a single word that is still considered a legitimate
        /// input for hyphenation. This value is suggested by Stefanie Schiller - the NLG expert when
        /// considering a theoretical example of a German compound word which consists of 12 compound
        /// segments. The following is that word.
        ///
        /// "DONAUDAMPFSCHIFFAHRTSELEKTRIZITAETENHAUPTBETRIEBSWERKBAUUNTERBEAMTENGESELLSCHAFT"
        ///
        /// [Wchao, 3/15/2006]
        /// </summary>
        private const int MaxCchWordToHyphenate = 80;

        #endregion

        #region Properties
        internal FormatSettings Settings
        {
            get { return _settings; }
        }

        internal ParaProp Pap
        {
            get { return _settings.Pap; }
        }

        internal int CpFirst
        {
            get { return _cpFirst; }
        }

        internal SpanVector PlsrunVector
        {
            get { return _plsrunVector; }
        }

        internal ArrayList LsrunList
        {
            get { return _lsrunList; }
        }

        internal int FormatWidth
        {
            get { return _formatWidth; }
        }

        internal int CchEol
        {
            get { return _cchEol; }
            set { _cchEol = value; }
        }
        #endregion
    }


    /// <summary>
    /// Bidi state that applie across line. If no preceding state is available internally,
    /// it calls back to the client to obtain additional Bidi control and explicit embedding level.
    /// </summary>
    internal sealed class BidiState : Bidi.State
    {
        public BidiState(FormatSettings settings, int cpFirst)
            : this(settings, cpFirst, null)
        {
        }

        public BidiState(FormatSettings settings, int cpFirst, TextModifierScope modifierScope)
            : base (settings.Pap.RightToLeft)
        {
            _settings = settings;
            _cpFirst = cpFirst;

            NumberClass = DirectionClass.ClassInvalid;
            StrongCharClass = DirectionClass.ClassInvalid;


            // find the top most scope that has the direction embedding
            while ( modifierScope != null && !modifierScope.TextModifier.HasDirectionalEmbedding)
            {
                modifierScope = modifierScope.ParentScope;
            }

            if (modifierScope != null)
            {
                _cpFirstScope = modifierScope.TextSourceCharacterIndex;

                // Initialize Bidi stack base on modifier scope
                Bidi.BidiStack stack = new Bidi.BidiStack();
                stack.Init(LevelStack);

                ushort overflowLevels = 0;
                InitLevelStackFromModifierScope(stack, modifierScope, ref overflowLevels);

                LevelStack = stack.GetData();
                Overflow = overflowLevels;
            }
        }


        /// <summary>
        /// Set the default last strongs when an embedding level is changed such that
        /// ambiguous characters (i.e. characters with null or InvariantCulture) at the beginning
        /// of the current embedding level can be resolved correctly.
        /// </summary>
        internal void SetLastDirectionClassesAtLevelChange()
        {
            if ((CurrentLevel & 1) == 0)
            {
                LastStrongClass = DirectionClass.Left;
                LastNumberClass = DirectionClass.Left;
            }
            else
            {
                LastStrongClass = DirectionClass.ArabicLetter;
                LastNumberClass = DirectionClass.ArabicNumber;
            }
        }

        internal byte CurrentLevel
        {
            get { return Bidi.BidiStack.GetMaximumLevel(LevelStack); }
        }


        /// <summary>
        /// Method to get the last number class overridden by bidi algorithm implementer
        /// </summary>
        public override DirectionClass LastNumberClass
        {
            get
            {
                if (this.NumberClass == DirectionClass.ClassInvalid )
                {
                    GetLastDirectionClasses();
                }

                return this.NumberClass;
            }

            set { this.NumberClass = value; }
        }


        /// <summary>
        /// Method to get the last strong class overridden by bidi algorithm implementer
        /// </summary>
        public override DirectionClass LastStrongClass
        {
            get
            {
                if (this.StrongCharClass == DirectionClass.ClassInvalid)
                {
                    GetLastDirectionClasses();
                }
                return this.StrongCharClass;
            }

            set
            {
                this.StrongCharClass = value;
                this.NumberClass = value;
            }
        }


        /// <summary>
        /// Last strong class not found internally, call out to client
        /// </summary>
        private void GetLastDirectionClasses()
        {
            DirectionClass  strongClass = DirectionClass.ClassInvalid;
            DirectionClass  numberClass = DirectionClass.ClassInvalid;

            // It is a flag to indicate whether to continue calling GetPrecedingText.
            // Because Bidi algorithm works within a paragraph only, we should terminate the
            // loop at paragraph boundary and fall back to the appropriate defaults.

            bool continueScanning = true;

            while (continueScanning && _cpFirst > _cpFirstScope)
            {
                TextSpan<CultureSpecificCharacterBufferRange> textSpan = _settings.GetPrecedingText(_cpFirst);
                CultureSpecificCharacterBufferRange charString = textSpan.Value;

                if (textSpan.Length <= 0)
                {
                    break;  // stop when preceding text span has length 0.
                }

                if (!charString.CharacterBufferRange.IsEmpty)
                {
                    continueScanning = Bidi.GetLastStongAndNumberClass(
                        charString.CharacterBufferRange,
                        ref strongClass,
                        ref numberClass
                        );

                    if (strongClass != DirectionClass.ClassInvalid)
                    {
                        this.StrongCharClass = strongClass;

                        if (this.NumberClass == DirectionClass.ClassInvalid)
                        {
                            if (numberClass == DirectionClass.EuropeanNumber)
                            {
                                // Override EuropeanNumber class as appropriate.
                                numberClass = GetEuropeanNumberClassOverride(CultureMapper.GetSpecificCulture(charString.CultureInfo));
                            }

                            this.NumberClass = numberClass;
                        }

                        break;
                    }
                }

                _cpFirst -= textSpan.Length;
            }


            // If we don't have the strong class and/or number class, select appropriate defaults
            // according to the base bidi level.
            //
            // To determine the base bidi level, we look at bit 0 if the LevelStack. This is NOT
            // an even/odd test. LevelStack is an array of bits corresponding to all of the bidl
            // levels on the stack. Thus, bit 0 is set if and only if the base bidi level is zero,
            // i.e., it's a left-to-right paragraph.

            if(strongClass == DirectionClass.ClassInvalid)
            {
                this.StrongCharClass = ((CurrentLevel & 1) == 0) ? DirectionClass.Left : DirectionClass.ArabicLetter;
            }

            if(numberClass == DirectionClass.ClassInvalid)
            {
                this.NumberClass = ((CurrentLevel & 1) == 0) ? DirectionClass.Left : DirectionClass.ArabicNumber;
            }
        }

        /// <summary>
        /// Walk the TextModifierScope to reinitialize the bidi stack.
        /// We push to bidi-stack from the earliest directional modifier (i.e. from bottom of the
        /// the scope chain onwards). We use a stack to reverse the scope chain first.
        /// </summary>
        private static void InitLevelStackFromModifierScope(
            Bidi.BidiStack    stack,
            TextModifierScope scope,
            ref ushort        overflowLevels
            )
        {
            Stack<TextModifier> directionalEmbeddingStack = new Stack<TextModifier>(32);

            for (TextModifierScope currentScope = scope; currentScope != null; currentScope = currentScope.ParentScope)
            {
                if (currentScope.TextModifier.HasDirectionalEmbedding)
                {
                    directionalEmbeddingStack.Push(currentScope.TextModifier);
                }
            }

            while (directionalEmbeddingStack.Count > 0)
            {
                TextModifier modifier = directionalEmbeddingStack.Pop();

                if (overflowLevels > 0)
                {
                    // Bidi level stack overflows. Just increment the bidi stack overflow number
                    overflowLevels ++;
                }
                else if (!stack.Push(modifier.FlowDirection == FlowDirection.LeftToRight))
                {
                    // Push stack not successful. Stack starts to overflow.
                    overflowLevels = 1;
                }
}
        }

        /// <summary>
        /// Obtain the explict direction class of European number based on culture and current flow direction.
        /// European numbers in Arabic/Farsi culture and RTL flow direction are to be considered as Arabic numbers.
        /// </summary>
        internal DirectionClass GetEuropeanNumberClassOverride(CultureInfo cultureInfo)
        {
            if (   cultureInfo != null
                 &&(   (cultureInfo.LCID & 0xFF) == 0x01 // Arabic culture
                    || (cultureInfo.LCID & 0xFF) == 0x29 // Farsi culture
                   )
                 && (CurrentLevel & 1) != 0 // RTL flow direction
                )
            {
                return DirectionClass.ArabicNumber;
            }

            return DirectionClass.EuropeanNumber;
        }

        private FormatSettings  _settings;
        private int             _cpFirst;
        private int             _cpFirstScope; // The first Cp of the current scope. GetLastStrong() should not go beyond it.
    }
}
