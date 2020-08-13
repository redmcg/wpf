// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.




using System;
using System.Security;
using System.Globalization;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using MS.Internal.Shaping;
using MS.Internal.Generic;
using MS.Internal;
using Common.TextFormatting;


namespace Managed.TextFormatting
{
    /// <summary>
    /// Formatting state of full text
    /// </summary>
    internal sealed class FullTextState
    {
        private TextStore           _store;                     // formatting store for main text
        private TextStore           _markerStore;               // store specifically for marker
        private StatusFlags         _statusFlags;               // status flags
        private int                 _cpMeasured;                // number of CP successfully measured and fit within column width
        private int                 _lscpHyphenationLookAhead;  // LSCP after the last character examined by the hyphenation code.
        private bool                _isSideways;


        [Flags]
        private enum StatusFlags
        {
            None            = 0,
            VerticalAdjust  = 0x00000001,   // vertical adjustment on some runs required
            ForceWrap       = 0x00000002,   // force wrap
            KeepState       = 0x00000040,   // state should be kept in the line
        }


        /// <summary>
        /// A word smaller than seven characters does not require hyphenation. This is based on 
        /// observation in real Western books and publications. Advanced research on readability
        /// seems to support this finding. It suggests that the 'fovea' which is the center
        /// point of our vision, can only see three to four letters before and after the center
        /// of a word.
        /// </summary>
        /// <remarks>
        /// "It has been known for over 100 years that when we read, our eyes don't move smoothly 
        /// across the page, but rather make discrete jumps from word to word. We fixate on a word 
        /// for a period of time, roughly 200-250ms, then make a ballistic movement to another word. 
        /// These movements are called saccades and usually take 20-35ms. Most saccades are forward 
        /// movements from 7 to 9 letters.
        /// ...
        /// During a single fixation, there is a limit to the amount of information that can be 
        /// recognized. The fovea, which is the clear center point of our vision, can only see 
        /// three to four letters to the left and right of fixation at normal reading distances."
        /// 
        /// ["The Science of Word Recognition" by Kevin Larson; July 2004]
        /// </remarks>
        private const int MinCchWordToHyphenate = 7;


        /// <summary>
        /// Create a fulltext state object from formatting info for subsequent fulltext formatting
        /// e.g. fulltext line, mix/max computation, potential breakpoints for optimal paragraph formatting.
        /// </summary>
        internal static FullTextState Create(
            FormatSettings      settings,
            int                 cpFirst,
            int                 finiteFormatWidth
            )
        {
            // prepare text stores
            TextStore store = new TextStore(
                settings, 
                cpFirst, 
                0, 
                settings.GetFormatWidth(finiteFormatWidth)
                );

            ParaProp pap = settings.Pap;
            TextStore markerStore = null;

            if(     pap.FirstLineInParagraph
                &&  pap.TextMarkerProperties != null
                &&  pap.TextMarkerProperties.TextSource != null)
            {
                // create text store specifically for marker
                markerStore = new TextStore(
                    // create specialized settings for marker store e.g. with marker text source
                    new FormatSettings(
                        settings.Formatter,
                        pap.TextMarkerProperties.TextSource,
                        new TextRunCacheImp(),   // no cross-call run cache available for marker store
                        pap,                     // marker by default use the same paragraph properties
                        null,                    // not consider previousLineBreak
                        true,                    // isSingleLineFormatting
                        settings.TextFormattingMode,
                        settings.IsSideways
                        ),
                    0,                           // marker store always started with cp == 0
                    TextStore.LscpFirstMarker,   // first lscp value for marker text
                    Constants.IdealInfiniteWidth // formatWidth 
                    );
            }

            // construct a new fulltext state object
            return new FullTextState(store, markerStore, settings.IsSideways);
        }


        /// <summary>
        /// Construct full text state for formatting
        /// </summary>
        private FullTextState(
            TextStore       store,
            TextStore       markerStore,
            bool isSideways
            )
        {
            _isSideways = isSideways;
            _store = store;
            _markerStore = markerStore;
        }


        /// <summary>
        /// Number of client CP for which we know the nominal widths fit in the
        /// available margin (i.e., because we've measured them).
        /// </summary>
        internal int CpMeasured 
        {
            get 
            { 
                return _cpMeasured; 
            }
            set 
            { 
                _cpMeasured = value; 
            }
        }


        /// <summary>
        /// LSCP immediately after the last LSCP examined by hyphenation code while being run.
        /// This value is used to calculate a more precise DependentLength value for the line
        /// ended by automatic hyphenation. 
        /// </summary>
        internal int LscpHyphenationLookAhead
        {
            get
            {
                return _lscpHyphenationLookAhead;
            }
        }

        internal TextFormattingMode TextFormattingMode
        {
            get
            {
                return Formatter.TextFormattingMode;
            }
        }

        internal bool IsSideways
        {
            get
            {
                return _isSideways;
            }
        }

        /// <summary>
        /// Set tab stops
        /// </summary>
        internal void SetTabs(TextFormatterContext context)
        {
			throw new NotImplementedException("FullTextState.SetTabs");
        }

        /// <summary>
        /// Get distance from the start of main text to the end of marker
        /// </summary>
        /// <remarks>
        /// Positive distance is filtered out. Marker overlapping the main text is not supported.
        /// </remarks>
        internal int GetMainTextToMarkerIdealDistance()
        {
            if (_markerStore != null)
            {
                return Math.Min(0, TextFormatterImp.RealToIdeal(_markerStore.Pap.TextMarkerProperties.Offset) - _store.Settings.TextIndent);
            }
            return 0;
        }

        
        /// <summary>
        /// Convert the specified external CP to LSCP corresponding to a possible break position for optimal break.
        /// </summary>
        /// <remarks>
        /// There is a generic issue that one CP could map to multiple LSCPs when it comes to
        /// lsrun that occupies no actual CP. Such lsrun is generated by line layout during
        /// formatting to accomplsih specific purpose i.e. run representing open and close
        /// reverse object or a fake-linebreak lsrun.
        /// 
        /// According to SergeyGe and Antons, LS will never breaks immediately after open reverse
        /// or immediately before close reverse. It also never break before a linebreak character. 
        ///
        /// Therefore, it is safe to make an assumption here that one CP will indeed map to one 
        /// LSCP given being the LSCP before the open reverse and the LSCP after the close reverse.
        /// Never the vice-versa.
        ///
        /// This is the reason why the loop inside this method may overread the PLSRUN span by
        /// one span at the end. The loop is designed to skip over all the lsruns with zero CP
        /// which is not the open reverse.
        /// 
        /// This logic is exactly the same as the one used by FullTextLine.PrefetchLSRuns. Any
        /// attempt to change it needs to be thoroughly reviewed. The same logic is to be applied
        /// accordingly on PrefetchLSRuns.
        /// 
        /// [Wchao, 5-24-2005]
        /// 
        /// </remarks>
        internal int GetBreakpointInternalCp(int cp)
        {
			throw new NotImplementedException("FullTextState.GetBreakpointInernalCp");
        }


        /// <summary>
        /// Get the lexical chunk the specified current LSCP is within.
        /// </summary>
        private LexicalChunk GetChunk(
            TextLexicalService      lexicalService,
            int                     lscpCurrent,
            int                     lscchLim,
            bool                    isCurrentAtWordStart,
            out int                 lscpChunk,
            out int                 lscchChunk
            )
        {
            int lscpStart = lscpCurrent;
            int lscpLim = lscpStart + lscchLim;

            int cpFirst = _store.CpFirst;

            if (lscpStart > lscpLim)
            {
                // Start is always before limit
                lscpStart = lscpLim;
                lscpLim = lscpCurrent;
            }

            LexicalChunk chunk = new LexicalChunk();
            int cchWordMax;
            CultureInfo textCulture;
            SpanVector<int> textVector;

            char[] rawText = _store.CollectRawWord(
                lscpStart,
                isCurrentAtWordStart,
                _isSideways,
                out lscpChunk,
                out lscchChunk,
                out textCulture,
                out cchWordMax,
                out textVector
                );

            if (    rawText != null
                &&  cchWordMax >= MinCchWordToHyphenate
                &&  lscpLim < lscpChunk + lscchChunk
                &&  textCulture != null 
                &&  lexicalService != null
                &&  lexicalService.IsCultureSupported(textCulture)
                )
            {
                // analyze the chunk and produce the lexical chunk to cache
                TextLexicalBreaks breaks = lexicalService.AnalyzeText(
                    rawText,
                    rawText.Length,
                    textCulture
                    );

                if (breaks != null)
                {
                    chunk = new LexicalChunk(breaks, textVector);
                }
            }

            return chunk;
        }


        /// <summary>
        /// Get a text store containing the specified plsrun
        /// </summary>
        internal TextStore StoreFrom(Plsrun plsrun)
        {
            return TextStore.IsMarker(plsrun) ? _markerStore : _store;
        }


        /// <summary>
        /// Get a text store containing the specified lscp
        /// </summary>
        internal TextStore StoreFrom(int lscp)
        {
            return lscp < 0 ? _markerStore : _store;
        }


        /// <summary>
        /// Flag indicating whether vertical adjustment of some runs is required
        /// </summary>
        internal bool VerticalAdjust
        {
            get { return (_statusFlags & StatusFlags.VerticalAdjust) != 0; }
            set 
            {
                if (value)
                    _statusFlags |= StatusFlags.VerticalAdjust; 
                else
                    _statusFlags &= ~StatusFlags.VerticalAdjust;
            }
        }


        /// <summary>
        /// Flag indicating whether force wrap is required
        /// </summary>
        internal bool ForceWrap
        {
            get { return (_statusFlags & StatusFlags.ForceWrap) != 0; }
            set 
            {
                if (value)
                    _statusFlags |= StatusFlags.ForceWrap; 
                else
                    _statusFlags &= ~StatusFlags.ForceWrap;
            }
        }


        /// <summary>
        /// Flag indicating whether state should be kept in the line
        /// </summary>
        internal bool KeepState
        {
            get { return (_statusFlags & StatusFlags.KeepState) != 0; }
        }


        /// <summary>
        /// Formatting store for main text
        /// </summary>
        internal TextStore TextStore
        {
            get { return _store; }
        }


        /// <summary>
        /// Formatting store for marker text
        /// </summary>
        internal TextStore TextMarkerStore
        {
            get { return _markerStore; }
        }


        /// <summary>
        /// Current formatter
        /// </summary>
        internal TextFormatterImp Formatter
        {
            get { return _store.Settings.Formatter; }
        }


        /// <summary>
        /// formattng ideal width
        /// </summary>
        internal int FormatWidth
        {
            get { return _store.FormatWidth; }
        }
    }
}
