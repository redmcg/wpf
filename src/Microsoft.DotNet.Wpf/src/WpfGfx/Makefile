
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
	core/sw \
	../Shared/Tracing/native \
	../../../../wine

all: $(OBJDIR)/wpfgfx_cor3.dll
.PHONY: all

CXX_FLAGS=-c -Wall -Wno-format -Wno-unknown-pragmas -Wno-endif-labels -Wno-reorder -Wno-conversion-null -Wno-overloaded-virtual -Wno-inline-new-delete -Wno-ignored-pragma-optimize -Wno-undefined-inline -Wno-inconsistent-missing-override -Wno-deprecated-register -Wno-undefined-var-template -Wno-switch -Wno-missing-braces -Wno-parentheses-equality -Wno-implicit-int-float-conversion -Werror -fms-extensions -municode -DD3D9EXTENSIONS -DNOASM -D_UNICODE $(INCLUDE_DIRS:%=-I%)

LINK_FLAGS=-lgdi32 -lntdll -ld3dx9 -lwindowscodecs

$(OBJDIR)/Shared/%.cpp.o: ../Shared/%.cpp
	mkdir -p $(@D)
	$(MINGW)-g++ -o $@ $(CXX_FLAGS) $<

$(OBJDIR)/%.cpp.o: %.cpp
	mkdir -p $(@D)
	$(MINGW)-g++ -o $@ $(CXX_FLAGS) $<

$(OBJDIR)/%.cxx.o: %.cxx
	mkdir -p $(@D)
	$(MINGW)-g++ -o $@ $(CXX_FLAGS) $<

$(OBJDIR)/wpfgfx_cor3.dll: $(C_SRCS:%=$(OBJDIR)/%.o)
	$(MINGW)-g++ -o $@ -shared -Wl,--kill-at $(LINK_FLAGS) $(C_SRCS:%=$(OBJDIR)/%.o)
