
COMPILE_LIB=csc -target:library

LIB_FLAGS=-unsafe -delaysign+ -keyfile:../../reactive.pub -langversion:8.0 -nostdlib -noconfig

LIB_REFS=-r:mscorlib.dll -r:System.dll -r:System.Xml.dll -r:System.Core.dll

LIB_REF_FILES=../../src/System.Xaml/System.Xaml.dll ../../src/WindowsBase/WindowsBase.dll ../../src/PresentationCore/PresentationCore.dll ../PresentationFramework/PresentationFramework-PresentationUI-api-cycle.dll

LIB_SRCS= \
	AssemblyVersion.cs \
	PresentationUI.cs \
	PresentationUI.internals.cs \
	../../src/Shared/RefAssemblyAttrs.cs

all: PresentationUI-PresentationFramework-impl-cycle.dll

../PresentationFramework/PresentationFramework-PresentationUI-api-cycle.dll: ../PresentationFramework/*.cs
	+$(MAKE) -C $(@D) -f PresentationFramework-PresentationUI-api-cycle.make
	touch $@

PresentationUI-PresentationFramework-impl-cycle.dll: $(LIB_SRCS) $(LIB_REF_FILES)
	export TMPDIR=$$(mktemp -d); $(COMPILE_LIB) $(LIB_SRCS) $(LIB_FLAGS) $(LIB_REFS) $(LIB_REF_FILES:%=-r:%) -out:$$TMPDIR/PresentationUI.dll && mv $$TMPDIR/PresentationUI.dll $@ && rmdir $$TMPDIR
