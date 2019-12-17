
COMPILE_LIB=csc -target:library

LIB_FLAGS=-unsafe -delaysign+ -keyfile:../../reactive.pub -langversion:8.0 -nostdlib -noconfig

LIB_REFS=-r:mscorlib.dll -r:System.dll -r:System.Xml.dll -r:System.Core.dll

LIB_REF_FILES=../../src/System.Xaml/System.Xaml.dll ../../src/WindowsBase/WindowsBase.dll ../../src/UIAutomation/UIAutomationTypes/UIAutomationTypes.dll ../../src/UIAutomation/UIAutomationProvider/UIAutomationProvider.dll ../../src/PresentationCore/PresentationCore.dll ../ReachFramework/ReachFramework-PresentationFramework-api-cycle.dll ../System.Printing/System.Printing-PresentationFramework-api-cycle.dll

LIB_SRCS= \
	AssemblyVersion.cs \
	System.Windows.Documents.FixedDocument.cs \
	System.Windows.Documents.FixedDocumentSequence.cs \
	System.Windows.Documents.FixedPage.cs \
	System.Windows.Controls.PageRange.cs \
	System.Windows.Controls.PageRangeSelection.cs \
	System.Windows.Documents.Serialization.Delegates.cs \
	System.Windows.Documents.Serialization.SerializerWriter.cs \
	System.Windows.Documents.Serialization.SerializerWriterCollator.cs

all: PresentationFramework-System.Printing-api-cycle.dll

../ReachFramework/ReachFramework-PresentationFramework-api-cycle.dll: ../ReachFramework/*.cs
	+$(MAKE) -C ../ReachFramework -f ReachFramework-PresentationFramework-api-cycle.make
	touch $@

../System.Printing/System.Printing-PresentationFramework-api-cycle.dll: ../System.Printing/*.cs
	+$(MAKE) -C ../System.Printing -f System.Printing-PresentationFramework-api-cycle.make
	touch $@

PresentationFramework-System.Printing-api-cycle.dll: $(LIB_SRCS) $(LIB_REF_FILES)
	export TMPDIR=$$(mktemp -d); $(COMPILE_LIB) $(LIB_SRCS) $(LIB_FLAGS) $(LIB_REFS) $(LIB_REF_FILES:%=-r:%) -out:$$TMPDIR/PresentationFramework.dll && mv $$TMPDIR/PresentationFramework.dll $@ && rmdir $$TMPDIR
