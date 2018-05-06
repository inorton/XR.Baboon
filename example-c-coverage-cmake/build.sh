rm -rf bld
mkdir -p bld
cd bld
cmake -DCMAKE_TOOLCHAIN_FILE=../gcov.toolchain.cmake ..
make -j
cd ..
find . -type f -name *.gcno
