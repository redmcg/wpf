
COMPILE_LIB=csc -target:library

LIB_FLAGS=-unsafe -delaysign+ -keyfile:../../../reactive.pub -define:AUTOMATION -langversion:8.0 -nostdlib -noconfig -lib:$(MONO_PREFIX)/lib/mono/4.5-api

LIB_REFS=-r:mscorlib.dll -r:System.dll -r:System.Xaml.dll -r:System.Xml.dll -r:System.Core.dll -r:WindowsBase.dll

LIB_PATH_REFS=../UIAutomationTypes/UIAutomationTypes.dll

LIB_SRCS=$(shell cat sources.list)

all: UIAutomationProvider.dll

SRID.generated.cs: Resources/Strings.resx $(RESX2SRID)
	mono $(RESX2SRID) $< SRID > $@ || rm -f $@

%.resources: %.resx
	resgen $< $@

EMBEDDED_RESOURCES=$(shell cat embeddedresources.list)

EMBEDDED_RES_PATHS=$(foreach f,$(EMBEDDED_RESOURCES),Resources/$(f))

embeddedresources.flags: embeddedresources.list
	for i in `cat embeddedresources.list`; do printf -- '-resource:Resources/%s,%s\n' $$i `echo $$i|sed "s:/:.:g"|sed "s:Strings.resources:SR.resources:g"`; done > $@ || rm -f $@

UIAutomationProvider.dll: sources.list $(LIB_SRCS) embeddedresources.flags $(EMBEDDED_RES_PATHS) $(LIB_PATH_REFS)
	$(COMPILE_LIB) $(LIB_FLAGS) $(LIB_REFS) $(LIB_PATH_REFS:%=-r:%) -out:$@ @sources.list @embeddedresources.flags
	sn -R $@ ../../../mono.snk
