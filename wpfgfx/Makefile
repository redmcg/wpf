
C_SRCS=stubs.c main.c composition.c channel.c resource.c wrappers.c factory.c

H_SRCS=wpfgfx_private.h

%.o: $(SRCDIR)/%.c
	$(MINGW)-gcc -o $@ -D_WIN32_WINNT=0x0602 --std=c99 -c -Wall -Werror $^

all: wpfgfx-$(ARCH).dll
.PHONY: all

wpfgfx-$(ARCH).dll: $(C_SRCS:%.c=%.o) $(H_SRCS:%=$(SRCDIR)/%)
	$(MINGW)-gcc -o $@ -shared -Wl,--kill-at $(C_SRCS:%.c=%.o) -luuid
