XR_COV_LIB=XR.Mono.Cover/bin/Release/XR.Mono.Cover.dll
XR_COV_CON=covtool/bin/Release/covtool.exe

all: 
	git submodule update --init
	xbuild /property:Configuration=Release 

clean:
	rm -rf cov-gtk/obj
	rm -rf cov-gtk/bin
	rm -rf covtool/obj
	rm -rf covtool/bin
	rm -rf XR.Mono.Cover/obj
	rm -rf XR.Mono.Cover/bin
