cmake_minimum_required(VERSION 3.5)

project(gcov-c-example C)

add_executable(program test.c foo/file.c bar/file.c)
