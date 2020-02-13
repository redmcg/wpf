
.SUFFIXES: #disable all builtin rules

C_SRCS=$(shell cat sources.list)

INCLUDE_DIRS= \
	../Shared/inc \
	shared/inc \
	include \
	include/Generated \
	include/processed \
	common/DirectXLayer \
	common/DirectXLayer/interfaces \
	common/DirectXLayer/Factories \
	shared/util/UtilLib \
	../../../../wine

all: $(OBJDIR)/wpfgfx_cor3.dll
.PHONY: all

$(OBJDIR)/%.cpp.o: %.cpp
	mkdir -p $(@D)
	$(MINGW)-g++ -o $@ -c -Wall -Wno-format -Wno-unknown-pragmas -Werror -DD3D9EXTENSIONS $(INCLUDE_DIRS:%=-I%) $<

$(OBJDIR)/wpfgfx_cor3.dll: $(C_SRCS:%=$(OBJDIR)/%.o)
	$(MINGW)-g++ -o $@ -shared -Wl,--kill-at $(C_SRCS:%=$(OBJDIR)/%.o)
