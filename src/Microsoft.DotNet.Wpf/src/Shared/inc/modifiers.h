// Replacements for MSVC-specific modifiers on mingw
#pragma once

#ifndef _MSC_VER
#define __allocator
#define __annotation(...)
#define __bcount(x)
#define __deref_bcount(x)
#define __deref_out
#define __deref_out_ecount(x)
#define __deref_out_range(x,y)
#define __drv_aliasesMem
#define __drv_allocatesMem(x)
#define __drv_freesMem(x)
#define __drv_functionClass(x)
#define __drv_sameIRQL
#define __in
#define __in_bcount(x)
#define __in_bcount_opt(x)
#define __in_ecount(x)
#define __in_ecount_opt(x)
#define __in_opt
#define __in_xcount(x)
#define __inout
#define __inout_ecount(x)
#define __out
#define __out_bcount(x)
#define __out_bcount_full(x)
#define __out_ecount(x)
#define __out_ecount_opt(x)
#define __out_xcount(x)
#define __out_xcount_opt(x)
#define __post_invalid
#define __success(x)
#define _Post_satisfies_(x)
#endif
