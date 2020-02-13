
.SUFFIXES: #disable all builtin rules

C_SRCS=$(shell cat sources.list)

INCLUDE_DIRS= \
	../Shared/inc \
	shared/inc \
	include \
	include/Generated \
	include/processed \
	../../../../wine

all: $(OBJDIR)/wpfgfx_cor3.dll
.PHONY: all

$(OBJDIR)/%.cpp.o: %.cpp
	mkdir -p $(@D)
	$(MINGW)-g++ -o $@ -c -Wall -Wno-format -Werror $(INCLUDE_DIRS:%=-I%) $<

$(OBJDIR)/wpfgfx_cor3.dll: $(C_SRCS:%=$(OBJDIR)/%.o)
	$(MINGW)-g++ -o $@ -shared -Wl,--kill-at $(C_SRCS:%=$(OBJDIR)/%.o)
