// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//+-----------------------------------------------------------------------------
//

//

#include "precomp.hpp"

// C4356: 'TMilRect<TBase,TBaseRect,unique>::sc_rcEmpty' : static data member cannot be initialized via derived class
#pragma warning(disable:4356)

template<>
const TMilRect<float, MilRectF>::Rect_t TMilRect<float, MilRectF>::sc_rcEmpty(
    0, 0,
    0, 0,
    LTRB_Parameters
    );

template<>
const TMilRect<float, MilRectF>::Rect_t TMilRect<float, MilRectF>::sc_rcInfinite(
    -FLT_MAX, -FLT_MAX,
     FLT_MAX,  FLT_MAX,
    LTRB_Parameters
    );


template<>
const TMilRect<INT, MilRectL, RectUniqueness::_CMilRectL_>::Rect_t TMilRect<INT, MilRectL, RectUniqueness::_CMilRectL_>::sc_rcEmpty(
    0, 0,
    0, 0,
    LTRB_Parameters
    );

template<>
const TMilRect<INT, MilRectL, RectUniqueness::_CMilRectL_>::Rect_t TMilRect<INT, MilRectL, RectUniqueness::_CMilRectL_>::sc_rcInfinite(
    -LONG_MAX-1, -LONG_MAX-1,
    LONG_MAX, LONG_MAX,
    LTRB_Parameters
    );

template<>
const CMilRectU::Rect_t CMilRectU::sc_rcEmpty(
    0, 0,
    0, 0,
    LTRB_Parameters
    );

template<>
const CMilRectU::Rect_t CMilRectU::sc_rcInfinite(
    0, 0,
    ULONG_MAX, ULONG_MAX,
    LTRB_Parameters
    );




