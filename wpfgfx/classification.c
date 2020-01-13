
#include <windows.h>

#include <pshpack1.h>
typedef struct CharacterAttribute {
	BYTE Script;
	BYTE ItemClass;
	USHORT Flags;
	BYTE BreakType;
	BYTE BiDi;
	SHORT LineBreak;
} CharacterAttribute;
#include <poppack.h>

#include "class_table.h"

typedef struct RawClassificationTables {
	const USHORT * const * const * UnicodeClasses;
	const CharacterAttribute * CharacterAttributes;
	/* other fields are unused */
} RawClassificationTables;

void WINAPI MILGetClassificationTables(RawClassificationTables* ct)
{
	ct->UnicodeClasses = UnicodeClassTable;
	ct->CharacterAttributes = CharacterAttributeTable;
}

