
PERL=perl

PackageXmlStringTable.cs: MS/Internal/IO/Packaging/PackageXmlNamespaces.txt MS/Internal/IO/Packaging/PackageXmlStrings.txt ../../../../eng/WpfArcadeSdk/tools/GenXmlStringTable.pl
	$(PERL) ../../../../eng/WpfArcadeSdk/tools/GenXmlStringTable.pl -o $@ -n MS/Internal/IO/Packaging/PackageXmlNamespaces.txt -x MS/Internal/IO/Packaging/PackageXmlStrings.txt -e MS.Internal.IO.Packaging.PackageXmlEnum -c MS.Internal.IO.Packaging.PackageXmlStringTable
