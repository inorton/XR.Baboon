#!/bin/bash
set -e
START=`pwd`
pushd covtool/bin/
MONO_PATH=. mkbundle -c -o host.c -oo covem.o  --deps covem.exe


SQLITE=/usr/lib/i386-linux-gnu/libsqlite3.a

if [ ! -f $SQLITE ]
then
SQLITE=`pkg-config --libs sqlite3`
fi

CMD="cc -o covem -Wall `pkg-config --cflags mono-2` host.c \
 `pkg-config --libs-only-L mono-2` -Wl,-Bstatic -lmono-2.0 -Wl,-Bdynamic \
 `pkg-config --libs-only-l mono-2 | sed -e "s/\-lmono-2.0 //"` $SQLITE covem.o"

echo $CMD
$CMD


cp covem $START/.
popd


#
# cc -o covem -Wall `pkg-config --cflags mono-2` temp.c  `pkg-config --libs-only-L mono-2` -Wl,-Bstatic -lmono-2.0 -Wl,-Bdynamic `pkg-config --libs-only-l mono-2 | sed -e "s/\-lmono-2.0 //"` temp.o
#
#
#

