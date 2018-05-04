
program: test.o
	gcc -fprofile-arcs -ftest-coverage test.o -o program

test.o: test.c
	gcc -Wall -fprofile-arcs -ftest-coverage -c test.c

clean:
	rm -f test.o
	rm -f program
	rm -f *.gcda
	rm -f *.gcno
	rm -f *.gcov

test: program
	./program foo bar baz bog
	gcov -spaufib test.gcno
