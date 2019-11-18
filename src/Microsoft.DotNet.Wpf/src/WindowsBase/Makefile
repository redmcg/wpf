
COMPILE_LIB=csc -target:library

LIB_FLAGS=-unsafe -delaysign+ -keyfile:../../ecma.pub -define:BASE_NATIVEMETHODS -define:WINDOWS_BASE -langversion:8.0 -nostdlib -noconfig -lib:$(MONO_PREFIX)/lib/mono/4.5-api

LIB_REFS=-r:mscorlib.dll -r:System.dll -r:System.Xaml.dll -r:System.Xml.dll -r:System.Core.dll -r:System.Security.dll -r:System.Configuration.dll

LIB_SRCS=$(shell cat sources.list)

all: WindowsBase.dll

SRID.generated.cs: Resources/Strings.resx $(RESX2SRID)
	mono $(RESX2SRID) $< > $@ || rm -f $@

%.resources: %.resx
	resgen $< $@

PERL=perl

PackageXmlStringTable.cs: MS/Internal/IO/Packaging/PackageXmlNamespaces.txt MS/Internal/IO/Packaging/PackageXmlStrings.txt ../../../../eng/WpfArcadeSdk/tools/GenXmlStringTable.pl
	$(PERL) ../../../../eng/WpfArcadeSdk/tools/GenXmlStringTable.pl -o $@ -n MS/Internal/IO/Packaging/PackageXmlNamespaces.txt -x MS/Internal/IO/Packaging/PackageXmlStrings.txt -e MS.Internal.IO.Packaging.PackageXmlEnum -c MS.Internal.IO.Packaging.PackageXmlStringTable

EMBEDDED_RESOURCES=$(shell cat embeddedresources.list)

EMBEDDED_RES_PATHS=$(foreach f,$(EMBEDDED_RESOURCES),Resources/$(f))

embeddedresources.flags: embeddedresources.list
	for i in `cat embeddedresources.list`; do printf -- '-resource:Resources/%s,%s\n' $$i `echo $$i|sed "s:/:.:g"|sed "s:Strings.resources:SR.resources:g"`; done > $@ || rm -f $@

WindowsBase.dll: sources.list $(LIB_SRCS) embeddedresources.flags $(EMBEDDED_RES_PATHS) $(SIP_DLL) $(ACCESSIBILITY_DLL)
	$(COMPILE_LIB) $(LIB_FLAGS) $(LIB_REFS) -r:$(SIP_DLL) -r:$(ACCESSIBILITY_DLL) -out:$@ @sources.list @embeddedresources.flags
	sn -R $@ ../../mono.snk
