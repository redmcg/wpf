
COMPILE_LIB=csc -target:library

LIB_FLAGS=-unsafe -delaysign+ -keyfile:../../reactive.pub -langversion:8.0 -nostdlib -noconfig -define:REACHFRAMEWORK

LIB_REFS=-r:mscorlib.dll -r:System.dll -r:System.Xml.dll -r:System.Core.dll

LIB_SRCS= \
	AssemblyVersion.cs \
	System.Printing.PrintQueue.cs \
	../../src/Shared/RefAssemblyAttrs.cs

all: System.Printing-PresentationFramework-api-cycle.dll

System.Printing-PresentationFramework-api-cycle.dll: $(LIB_SRCS) $(LIB_REF_FILES)
	export TMPDIR=$$(mktemp -d); $(COMPILE_LIB) $(LIB_SRCS) $(LIB_FLAGS) $(LIB_REFS) $(LIB_REF_FILES:%=-r:%) -out:$$TMPDIR/System.Printing.dll && mv $$TMPDIR/System.Printing.dll $@ && rmdir $$TMPDIR
