// Replacements for MSVC-specific modifiers on mingw
#pragma once

#ifndef _MSC_VER

#define __allocator
#define __annotation(...) (void)0
#define __bound
#define __deref_bcount(x)
#define __deref_ecount(x)
#define __deref_in_range(x,y)
#define __deref_inout_ecount(x)
#define __deref_inout_ecount_opt(x)
#define __deref_inout_xcount(x)
#define __deref_opt_inout_ecount(x)
#define __deref_opt_inout_ecount_opt(x)
#define __deref_opt_out
#define __deref_opt_out_xcount_part(x,y)
#define __deref_out
#define __deref_out_bcount(x)
#define __deref_out_bcount_part(x,y)
#define __deref_out_ecount_opt(x)
#define __deref_out_ecount_part(x,y)
#define __deref_out_range(x,y)
#define __deref_out_xcount_opt(x)
#define __drv_functionClass(x)
#define __drv_sameIRQL
#define __ecount_opt(x)
#define __field_bcount(x)
#define __field_ecount_full(x)
#define __field_ecount_part(x,y)
#define __field_ecount_part_opt(x,y)
#define __field_range(x,y)
#define __in
#define __in_bcount_opt(x)
#define __in_ecount_opt(x)
#define __in_range(x,y)
#define __in_xcount(x)
#define __inout_bcount_part_opt(x,y)
#define __inout_ecount(x)
#define __inout_ecount_opt(x)
#define __inout_ecount_part_opt(x,y)
#define __nullterminated
#define __out
#define __out_ecount_opt(x)
#define __out_range(x,y)
#define __out_xcount(x)
#define __out_xcount_opt(x)
#define __out_xcount_part(x,y)
#define __override
#define __post_invalid
#define __success(x)
#define __typefix(x)
#define __xcount(x)

#ifndef __clang__
#define __bcount(x)
#define __deref_out_ecount(x)
#define __drv_aliasesMem
#define __drv_allocatesMem(x)
#define __drv_freesMem(x)
#define __ecount(x)
#define __if_exists(x) if (1)
#define __in_bcount(x)
#define __in_ecount(x)
#define __in_opt
#define __inout
#define __out_bcount(x)
#define __out_bcount_full(x)
#define __out_ecount(x)
#define __out_ecount_part(x,y)
#define __out_opt
#define _Post_satisfies_(x)
#endif // !defined(__clang__)

#endif // !defined(_MSC_VER)
