
RESX2SR=$(WINE_MONO_SRCDIR)/mono/mcs/class/lib/build-linux/resx2sr.exe

COMPILE_LIB=csc -target:library

LIB_FLAGS=-unsafe -delaysign+ -keyfile:../../ecma.pub -langversion:8.0 -nostdlib -noconfig -lib:$(MONO_PREFIX)/lib/mono/4.5-api

LIB_REFS=-r:mscorlib.dll -r:System.dll -r:System.Xml.dll -r:System.Core.dll

LIB_SRCS=$(shell cat sources.list)

all: System.Windows.Input.Manipulations.dll

SR.generated.cs: Resources/Strings.resx $(RESX2SR)
	mono $(RESX2SR) $< > $@ || rm -f $@

%.resources: %.resx
	resgen $< $@

EMBEDDED_RESOURCES=$(shell cat embeddedresources.list)

EMBEDDED_RES_PATHS=$(foreach f,$(EMBEDDED_RESOURCES),Resources/$(f))

embeddedresources.flags: embeddedresources.list
	for i in `cat embeddedresources.list`; do printf -- '-resource:Resources/%s,%s\n' $$i `echo $$i|sed "s:/:.:g"|sed "s:Strings.resources:SR.resources:g"`; done > $@ || rm -f $@

System.Windows.Input.Manipulations.dll: sources.list $(LIB_SRCS) embeddedresources.flags $(EMBEDDED_RES_PATHS)
	$(COMPILE_LIB) $(LIB_FLAGS) $(LIB_REFS) -out:$@ @sources.list @embeddedresources.flags
	sn -R $@ ../../mono.snk
