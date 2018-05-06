

program: foo/file.o bar/file.o test.o
	gcc -fprofile-arcs -ftest-coverage -o $@ $^

%.o: %.c
	gcc -I. -Wall -fprofile-arcs -ftest-coverage -c $< -o $@

clean:
	rm -f test.o
	rm -f program
	rm -f *.gcda
	rm -f *.gcno
	rm -f *.gcov

test: program
	./program foo bar baz bog
	gcov -spaufib test.gcno
