XR_COV_LIB=XR.Mono.Cover/bin/Release/XR.Mono.Cover.dll
XR_COV_CON=covtool/bin/Release/covtool.exe
INSTDIR=/usr/local/lib/baboon
BINDIR=/usr/local/bin

all:
	git submodule update --init
	xbuild /property:Configuration=Release 
	bash make_bundle

clean:
	rm -rf cov-gtk/obj
	rm -rf cov-gtk/bin
	rm -rf covtool/obj
	rm -rf covtool/bin
	rm -rf XR.Mono.Cover/obj
	rm -rf XR.Mono.Cover/bin

install:
	install -d $(INSTDIR) $(BINDIR)
	install -m 755 covem $(BINDIR)
	install covtool/bin/XR.Mono.Cover.dll $(INSTDIR)
	install covtool/bin/cov-gtk.exe $(INSTDIR)
	install covtool/bin/cov-html.exe $(INSTDIR)
	install scripts/cov-gtk $(BINDIR)
	install scripts/cov-html $(BINDIR)
