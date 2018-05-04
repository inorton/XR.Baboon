#include <stdio.h>

static void print_string(char * text) {
  printf("0x%p = %s\n", &text, text);
}

int main(int argc, char** argv) {
  int i;
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
