
COMPILE_LIB=csc -target:library

LIB_FLAGS=-unsafe -delaysign+ -keyfile:../../reactive.pub -langversion:8.0 -nostdlib -noconfig -define:REACHFRAMEWORK

LIB_REFS=-r:mscorlib.dll -r:System.dll -r:System.Xml.dll -r:System.Core.dll

LIB_REF_FILES=../../src/System.Xaml/System.Xaml.dll ../../src/WindowsBase/WindowsBase.dll ../../src/UIAutomation/UIAutomationTypes/UIAutomationTypes.dll ../../src/UIAutomation/UIAutomationProvider/UIAutomationProvider.dll ../../src/PresentationCore/PresentationCore.dll

LIB_SRCS= \
	AssemblyVersion.cs \
	System.Printing.PrintTicket.cs \
	System.Windows.Xps.Serialization.PrintTicketLevel.cs \
	../../src/Shared/RefAssemblyAttrs.cs

all: ReachFramework-PresentationFramework-api-cycle.dll

ReachFramework-PresentationFramework-api-cycle.dll: $(LIB_SRCS) $(LIB_REF_FILES)
	export TMPDIR=$$(mktemp -d); $(COMPILE_LIB) $(LIB_SRCS) $(LIB_FLAGS) $(LIB_REFS) $(LIB_REF_FILES:%=-r:%) -out:$$TMPDIR/ReachFramework.dll && mv $$TMPDIR/ReachFramework.dll $@ && rmdir $$TMPDIR
