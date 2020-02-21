
.SUFFIXES: #disable all builtin rules

C_SRCS=$(shell cat sources.list)

INCLUDE_DIRS= \
	../Shared/inc \
	shared/inc \
	include \
	include/Generated \
	include/processed \
	common \
	common/DirectXLayer \
	common/DirectXLayer/interfaces \
	common/DirectXLayer/factories \
	shared/inc \
	shared/util/utillib \
	core \
	../Shared/Tracing/native \
	../../../../wine

all: $(OBJDIR)/wpfgfx_cor3.dll
.PHONY: all

$(OBJDIR)/%.cpp.o: %.cpp
	mkdir -p $(@D)
	$(MINGW)-g++ -o $@ -c -Wall -Wno-format -Wno-unknown-pragmas -Wno-endif-labels -Wno-reorder -Werror -DD3D9EXTENSIONS $(INCLUDE_DIRS:%=-I%) $<

$(OBJDIR)/wpfgfx_cor3.dll: $(C_SRCS:%=$(OBJDIR)/%.o)
	$(MINGW)-g++ -o $@ -shared -Wl,--kill-at $(C_SRCS:%=$(OBJDIR)/%.o)
