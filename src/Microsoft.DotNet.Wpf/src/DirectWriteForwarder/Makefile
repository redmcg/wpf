
COMPILE_LIB=csc -target:library

LIB_FLAGS=-unsafe -keyfile:../../mono.snk -langversion:8.0

LIB_REFS=-r:mscorlib.dll -r:System.dll -r:System.Xml.dll -r:System.Core.dll

LIB_REF_FILES=../WindowsBase/WindowsBase.dll

LIB_SRCS=$(shell cat sources.list)

all: DirectWriteForwarder.dll

DirectWriteForwarder.dll: sources.list $(LIB_SRCS) $(LIB_REF_FILES)
	$(COMPILE_LIB) $(LIB_FLAGS) $(LIB_REFS) $(LIB_REF_FILES:%=-r:%) -out:$@ @sources.list
