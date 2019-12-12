
C_SRCS=stubs.c

%.o: $(SRCDIR)/%.c
	$(MINGW)-gcc -o $@ -c $^

all: wpfgfx-$(ARCH).dll
.PHONY: all

wpfgfx-$(ARCH).dll: $(C_SRCS:%.c=%.o)
	$(MINGW)-gcc -o $@ -shared $(C_SRCS:%.c=%.o)
