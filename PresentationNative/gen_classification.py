# Requires files from https://www.unicode.org/Public/zipped/12.1.0/UCD.zip in UCD directory

import ctypes

# table data
TABLE_SIZE = 0x100000

script_values = (ctypes.c_byte * TABLE_SIZE)()
itemclass_values = (ctypes.c_byte * TABLE_SIZE)()
bidi_values = (ctypes.c_byte * TABLE_SIZE)()
flags_values = (ctypes.c_ushort * TABLE_SIZE)()

character_attribute_values = []

# mapping information

bidi_categories = {
	'L': 0, #Left
	'R': 1, #Right
	'AN': 2, #Arabic Number
	'EN': 3, #European Number
	'AL': 4, #Arabic Letter
	'ES': 5, #European Separator
	'CS': 6, #Common separator
	'ET': 7, #European Terminator
	'NSM': 8, #Non-spacing mark
	'BN': 9, #Boundary neutral
	# 10: Generic neutral?
	'B': 11, #Paragraph separator
	'LRE': 12, #Left-to-right embedding
	'LRO': 13, #Left-to-right override
	'RLE': 14, #Right-to-left embedding
	'RLO': 15, #Right-to-left override
	'PDF': 16, #Pop directional format
	'S': 17, #Segment separator
	'WS': 18, #Whitespace
	'ON': 19, #Other neutral
	# unsupported:
	'LRI': 19,
	'RLI': 19,
	'FSI': 19,
	'PDI': 19,
}

script_mapping = {
    'Arabic': 0x1,
    'Armenian': 0x2,
    'Bengali': 0x3,
    'Bopomofo': 0x4,
    'Braille': 0x5,
    'Buginese': 0x6,
    'Buhid': 0x7,
    'Canadian_Aboriginal': 0x8,
    'Cherokee': 0x9,
    'Han': 0xA,
    'Coptic': 0xB,
    'Cypriot': 0xC,
    'Cyrillic': 0xD,
    'Deseret': 0xE,
    'Devanagari': 0xF,
    'Ethiopic': 0x10,
    'Georgian': 0x11,
    'Glagolitic': 0x12,
    'Gothic': 0x13,
    'Greek': 0x14,
    'Gujarati': 0x15,
    'Gurmukhi': 0x16,
    'Hangul': 0x17,
    'Hanunoo': 0x18,
    'Hebrew': 0x19,
    'Kannada': 0x1A,
    'Katakana': 0x1B,
    'Hiragana': 0x1B,
    'Kharoshthi': 0x1C,
    'Khmer': 0x1D,
    'Lao': 0x1E,
    'Latin': 0x1F,
    'Limbu': 0x20,
    'Linear_B': 0x21,
    'Malayalam': 0x22,
    'Mongolian': 0x24,
    'Myanmar': 0x26,
    'New_Tai_Lue': 0x27,
    'Ogham': 0x28,
    'Old_Italic': 0x29,
    'Old_Persian': 0x2A,
    'Oriya': 0x2B,
    'Osmanya': 0x2C,
    'Runic': 0x2D,
    'Shavian': 0x2E,
    'Sinhala': 0x2F,
    'Syloti_Nagri': 0x30,
    'Syriac': 0x31,
    'Tagalog': 0x32,
    'Tagbanwa': 0x33,
    'Tai_Le': 0x34,
    'Tamil': 0x35,
    'Telugu': 0x36,
    'Thaana': 0x37,
    'Thai': 0x38,
    'Tibetan': 0x39,
    'Tifinagh': 0x3A,
    'Ugaritic': 0x3B,
    'Yi': 0x3C,
}

complex_scripts = {
	'Arabic',
	'Bengali',
	'Braille',
	'Devanagari',
	'Gujarati',
	'Gurmukhi',
	'Hangul',
	'Hebrew',
	'Kannada',
	'Lao',
	'Malayalam',
	'Mongolian',
	'Old_Persian',
	'Oriya',
	'Tamil',
	'Telugu',
	'Thaana',
	'Thai',
	'Tibetan',
	'Sinhala',
	'Syriac',
	}

# process UnicodeData.txt
infile = open('UCD/UnicodeData.txt', 'r')
next_codevalue = 0

for line in infile:
	line = line.rstrip('\n').split(';')
	cp = int(line[0], 16)
	if cp >= TABLE_SIZE:
		break
	bidi = bidi_categories[line[4]]
	flags = 0
	if line[2] == 'Nd':
		script = 0x3D # Digit
		flags |= 0x100 # CharacterDigit
	elif line[2] == 'Cc':
		script = 0x3E # Control
		flags |= 0x1 # CharacterComplex
	elif line[2].startswith('L') and 'MATHEMATICAL' in line[1]:
		script = 0x23 # MathematicalAlphanumericSymbols
	elif line[1].startswith('MUSIC'):
		script = 0x25 # MusicalSymbols
	else:
		script = None
	if line[4] in ('L', 'R', 'AL'):
		itemclass = 0x5 #StrongClass
	elif line[2].startswith('M'):
		itemclass = 0x7 #SimpleMarkClass
	else:
		itemclass = None
	if line[4] not in ('L','EN','ES','CS','ET','NSM','BN','B','S','WS','ON'):
		flags |= 0x2 # CharacterRTL
	if line[4] == 'B':
		flags |= 0x4 # CharacterLineBreak
	if line[4] in ('L','EN','ES','CS','ET','ON'):
		flags |= 0x10 # CharacterFastText
	if line[4] == 'WS':
		flags |= 0x80 # CharacterSpace
	if line[2].startswith('L'):
		flags |= 0x800 # CharacterLetter
	while next_codevalue <= cp:
		if script is not None:
			script_values[next_codevalue] = script
		if itemclass is not None:
			itemclass_values[next_codevalue] = itemclass
		bidi_values[next_codevalue] = bidi
		flags_values[next_codevalue] |= flags
		next_codevalue += 1

infile.close()

# process Scripts.txt
infile = open('UCD/Scripts.txt', 'r')

unseen_scripts = set(script_mapping)

for line in infile:
	line = line.rstrip('\n')
	if not line.split('#')[0].strip():
		continue

	codepoints = line.split(';')[0].strip()
	if '..' in codepoints:
		a, b = codepoints.split('..')
		range_low = int(a, 16)
		range_high = int(b, 16) + 1
	else:
		range_low = int(codepoints, 16)
		range_high = range_low + 1

	uniscript = line.split(';')[1].split('#')[0].strip()
	script = script_mapping.get(uniscript, 0)
	unseen_scripts.discard(uniscript)

	flags = 0
	if uniscript in complex_scripts:
		flags |= 0x1 # CharacterComplex
	if uniscript == 'Han':
		flags |= 0x20 # CharacterIdeo

	for cp in range(range_low, range_high):
		if script_values[cp] == 0:
			script_values[cp] = script
		flags_values[cp] |= flags

if unseen_scripts:
	print("There are scripts in the script_mapping table not found in Scripts.txt:", unseen_scripts)

infile.close()

# hard-coded adjustments

itemclass_values[0x200d] = 0xa # zero-width joiner

for ch in range(0xd800,0xe000):
	flags_values[ch] |= 0x1 # CharacterComplex
for ch in range(0x202a,0x202f):
	flags_values[ch] |= 0x1
flags_values[0x200e] |= 0x1
flags_values[0x200f] |= 0x1

flags_values[0x2028] |= 0x4 # CharacterLineBreak
flags_values[0xb] |= 0x4
flags_values[0xc] |= 0x4

flags_values[0xfffb] |= 0x8 # CharacterFormatAnchor

flags_values[0x2028] |= 0x200 # CharacterParaBreak

flags_values[0xa] |= 0x400 # CharacterCRLF
flags_values[0xd] |= 0x400

# generate class_table.h

outfile = open('class_table.h', 'w')
outfile.write('// This file was generated by gen_classification.py\n\n')

for plane in range(0, TABLE_SIZE, 0x10000):
	plane_values = []
	for upperbyte in range(plane, plane + 0x10000, 0x100):
		pcc = []
		for ch in range(upperbyte, upperbyte + 0x100):
			character_attributes = (script_values[ch], itemclass_values[ch], bidi_values[ch], flags_values[ch])
			try:
				idx = character_attribute_values.index(character_attributes)
			except ValueError:
				idx = len(character_attribute_values)
				character_attribute_values.append(character_attributes)
			pcc.append(idx)
		if any(x != pcc[0] for x in pcc):
			plane_values.append('PCC_%x' % upperbyte)
			outfile.write('const USHORT PCC_%x[256] = {%s};\n' % (upperbyte, ','.join(str(x) for x in pcc)))
		else:
			plane_values.append('(USHORT*)%s' % pcc[0])
	outfile.write('const USHORT * const PLANE_%x[256] = {%s};\n' % (plane, ','.join(plane_values)))

outfile.write('const USHORT * const * const UnicodeClassTable[] = {%s};\n' % (','.join('PLANE_%x' % x for x in range(0, TABLE_SIZE, 0x10000))))

outfile.write('const CharacterAttribute CharacterAttributeTable[] = {\n')
outfile.write(',\n'.join('{%s,%s,0x%x,0,%s,0} /* %s */' % (script, itemclass, flags, bidi, n) for (n, (script, itemclass, bidi, flags)) in enumerate(character_attribute_values)));
outfile.write('};\n');

outfile.close()

# generate UnicodeClass.cs

outfile = open('UnicodeClass.cs', 'w')

outfile.write('// This file was generated by gen_classification.py\n\n')
outfile.write('namespace MS.Internal {\n');
outfile.write('    internal enum UnicodeClass : ushort {\n');
outfile.write('        Max = %s\n' % max(len(character_attribute_values), 0x1d8));
outfile.write('    };\n');
outfile.write('}\n');

outfile.close()
