#include <stdio.h>
#include "foo/file.h"
#include "bar/file.h"

static void print_string(char * text) {
  printf("0x%p = %s\n", &text, text);
}

int main(int argc, char** argv) {
  int i;

  printf("foo = %d\n", foofunction());
  printf("bar = %d\n", barfunc(argc));

  for (i = 0; i < argc; i++) {
    if ( i == argc - 1 ){
      printf("and lastly... ");
    }
    if ( i == 10 ){
      printf("ten!!\n");
    }

    print_string(argv[i]);
  }

  return 0;
}
