
COMPILE_EXE=csc

EXE_SRCS=xaml2cs.cs

all: xaml2cs.exe

xaml2cs.exe: $(EXE_SRCS)
	export TMPDIR=$$(mktemp -d); $(COMPILE_EXE) $(EXE_SRCS) -out:$$TMPDIR/xaml2cs.exe && mv $$TMPDIR/xaml2cs.exe $@ && rmdir $$TMPDIR
