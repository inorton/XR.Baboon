XR_COV_LIB=XR.Mono.Cover/bin/Release/XR.Mono.Cover.dll
XR_COV_CON=covtool/bin/Release/covtool.exe
INSTDIR=/usr/local/lib/baboon
BINDIR=/usr/local/bin

all:
	xbuild /property:Configuration=Release 

makebundle: all
	bash make_bundle.sh

clean:
	rm -rf cov-gtk/obj
	rm -rf cov-gtk/bin
	rm -rf cov-html/obj
	rm -rf cov-html/bin
	rm -rf cov-srchtml/obj
	rm -rf cov-srchtml/bin
	rm -rf covtool/obj
	rm -rf covtool/bin
	rm -rf XR.Mono.Cover/obj
	rm -rf XR.Mono.Cover/bin

install_generic:
	install -d $(DESTDIR)$(INSTDIR) $(DESTDIR)$(BINDIR)
	install covtool/bin/XR.Mono.Cover.dll $(DESTDIR)$(INSTDIR)
	install covtool/bin/cov-gtk.exe $(DESTDIR)$(INSTDIR) || true
	install covtool/bin/cov-html.exe $(DESTDIR)$(INSTDIR)
	install covtool/bin/cov-srchtml.exe $(DESTDIR)$(INSTDIR)
	install scripts/cov-gtk $(DESTDIR)$(BINDIR)
	install scripts/cov-html $(DESTDIR)$(BINDIR)
	install scripts/cov-srchtml $(DESTDIR)$(BINDIR)

installbundle: install_generic
	install -m 755 covem $(DESTDIR)$(BINDIR)

install: install_generic
	install covtool/bin/covem.exe $(DESTDIR)$(INSTDIR)
	install scripts/covem $(DESTDIR)$(BINDIR)
