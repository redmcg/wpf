
C_SRCS=stubs.c main.c composition.c channel.c resource.c factory.c hwndtarget.c

H_SRCS=wpfgfx_private.h

all: wpfgfx_cor3.dll
.PHONY: all

%.o: $(SRCDIR)/%.c
	$(MINGW)-gcc -o $@ -D_WIN32_WINNT=0x0602 --std=c99 -c -Wall -Werror $<

wpfgfx_cor3.dll: $(C_SRCS:%.c=%.o) $(H_SRCS:%=$(SRCDIR)/%)
	$(MINGW)-gcc -o $@ -shared -Wl,--kill-at $(C_SRCS:%.c=%.o) -luuid -lgdi32
