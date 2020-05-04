// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//------------------------------------------------------------------------------

//
//------------------------------------------------------------------------------

#ifndef _MSC_VER
// Wine Mono hack: There are way too many "overrides" in the wrong place, just ignore them.
#define override
#endif

#include <wpfsdl.h>
#include "std.h"
#include "d2d1.h"

#include <strsafe.h>

#include "common\common.h"
#include "scanop\scanop.h"

#include "BitmapDbgIO.h"

#include "glyph.h"

#include "geometry\geometry.h"

#include "api\api_include.h"

#include "targets\targets.h"

#include "sw\sw.h"

#include "hw\hw.h"

#include "resources\resources.h"

