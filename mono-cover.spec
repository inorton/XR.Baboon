#
# spec file for package mono-cover
#


Name:           mono-cover
Version:        0.1
Release:        1
Summary:        Code coverage tool for mono
License:        unknown
Group:          Development/Languages/Mono
Url:            https://github.com/inorton/XR.Baboon
Source0:        mono-cover-%{version}.tar.bz2
BuildRequires:  mono
BuildRequires:  mono-devel
BuildRequires:  bzip2
BuildRoot:      %{_tmppath}/%{name}-%{version}-build
%define _prefix /usr/local/

%description
XR.Mono.Cover is a code coverage tool for mono/.NET

%prep
%setup -q -n mono-cover-%{version}

%build
make

%install
%makeinstall

%post 
%postun

%files
%defattr(-,root,root)

%{_prefix}/lib/baboon
%{_prefix}/bin
