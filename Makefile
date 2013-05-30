XR_COV_LIB=XR.Mono.Cover/bin/Release/XR.Mono.Cover.dll
XR_COV_CON=covtool/bin/Release/covtool.exe
INSTDIR=/usr/local/lib/baboon
BINDIR=/usr/local/bin

all: 
	git submodule update --init
	xbuild /property:Configuration=Release 

covem: covtool/bin/covem.exe
	bash make_bundle

clean:
	rm -rf cov-gtk/obj
	rm -rf cov-gtk/bin
	rm -rf covtool/obj
	rm -rf covtool/bin
	rm -rf XR.Mono.Cover/obj
	rm -rf XR.Mono.Cover/bin

install: covem
	install -d $(INSTDIR) $(BINDIR)
	install -m 755 covem $(BINDIR)
