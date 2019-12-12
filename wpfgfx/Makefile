
C_SRCS=stubs.c main.c composition.c channel.c

%.o: $(SRCDIR)/%.c
	$(MINGW)-gcc -o $@ -D_WIN32_WINNT=0x0602 --std=c99 -c $^

all: wpfgfx-$(ARCH).dll
.PHONY: all

wpfgfx-$(ARCH).dll: $(C_SRCS:%.c=%.o)
	$(MINGW)-gcc -o $@ -shared -Wl,--kill-at $(C_SRCS:%.c=%.o)
