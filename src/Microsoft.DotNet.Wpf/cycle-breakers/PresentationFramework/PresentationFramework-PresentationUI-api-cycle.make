
COMPILE_LIB=csc -target:library

LIB_FLAGS=-unsafe -delaysign+ -keyfile:../../reactive.pub -langversion:8.0 -nostdlib -noconfig

LIB_REFS=-r:mscorlib.dll -r:System.dll -r:System.Xml.dll -r:System.Core.dll

LIB_SRCS= \
	AssemblyVersion.cs \
	System.Windows.Controls.DocumentViewer.cs \
	System.Windows.Controls.ToolBar.cs

all: PresentationFramework-PresentationUI-api-cycle.dll

PresentationFramework-PresentationUI-api-cycle.dll: $(LIB_SRCS) $(LIB_REF_FILES)
	export TMPDIR=$$(mktemp -d); $(COMPILE_LIB) $(LIB_SRCS) $(LIB_FLAGS) $(LIB_REFS) $(LIB_REF_FILES:%=-r:%) -out:$$TMPDIR/PresentationFramework.dll && mv $$TMPDIR/PresentationFramework.dll $@ && rmdir $$TMPDIR
