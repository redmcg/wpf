
COMPILE_LIB=csc -target:library

LIB_FLAGS=-unsafe -delaysign+ -keyfile:../../../reactive.pub -langversion:8.0 -nostdlib -noconfig

LIB_REFS=-r:mscorlib.dll -r:System.dll -r:System.Xml.dll -r:System.Core.dll

LIB_REF_FILES=../../../src/System.Xaml/System.Xaml.dll ../../../src/WindowsBase/WindowsBase.dll ../../../src/UIAutomation/UIAutomationTypes/UIAutomationTypes.dll ../../../src/UIAutomation/UIAutomationProvider/UIAutomationProvider.dll ../../../src/PresentationCore/PresentationCore.dll ../../../cycle-breakers/PresentationFramework/PresentationFramework-System.Printing-api-cycle.dll ../../../cycle-breakers/ReachFramework/ReachFramework-System.Printing-api-cycle.dll

LIB_SRCS= \
	AssemblyVersion.cs \
	System.Printing.cs \
	System.Printing.internals.cs \
	../../Shared/RefAssemblyAttrs.cs

all: System.Printing.dll

../../../cycle-breakers/ReachFramework/ReachFramework-System.Printing-api-cycle.dll: ../../../cycle-breakers/ReachFramework/*.cs
	+$(MAKE) -C $(@D) -f ReachFramework-System.Printing-api-cycle.make
	touch $@

../../../cycle-breakers/PresentationFramework/PresentationFramework-System.Printing-api-cycle.dll: ../../../cycle-breakers/System.Printing/*.cs
	+$(MAKE) -C $(@D) -f PresentationFramework-System.Printing-api-cycle.make
	touch $@

System.Printing.dll: $(LIB_SRCS) $(LIB_REF_FILES)
	export TMPDIR=$$(mktemp -d); $(COMPILE_LIB) $(LIB_SRCS) $(LIB_FLAGS) $(LIB_REFS) $(LIB_REF_FILES:%=-r:%) -out:$$TMPDIR/System.Printing.dll && mv $$TMPDIR/System.Printing.dll $@ && rmdir $$TMPDIR
