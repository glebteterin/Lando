using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if NET4_0
	[assembly: AssemblyTitle("Lando for .NET Framework 4")]
#elif NET4_5
	[assembly: AssemblyTitle("Lando for .NET Framework 4.5")]
#endif

[assembly: AssemblyDescription("Lando")]
[assembly: AssemblyCompany("Lando")]
[assembly: AssemblyProduct("Lando v1.0")]
[assembly: AssemblyVersion("1.0.3")]
[assembly: AssemblyCopyright("Copyright © 2013 by Gleb Teterin")]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]

[assembly: InternalsVisibleTo("Lando.UnitTests")]
