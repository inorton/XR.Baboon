CFLAGS=-Werror -Wall --coverage -g -O0
LDFLAGS=--coverage -g -O0


all: program


%.o: %.c
	gcc ${CFLAGS} -c $^ -o $@

program: foo/file.o bar/file.o test.o
	gcc ${LDFLAGS} $^ -o $@
	find . -type f -name "*.gcno"

test: program
	./program
	find . -type f -name "*.gcda"

clean:
	rm -f program test.o foo/*.o bar/*.o
	rm -f *.gcda *.gcno
	rm -rf foo/*.gcda foo/*.gcno bar/*.gcda bar/*.gcno
