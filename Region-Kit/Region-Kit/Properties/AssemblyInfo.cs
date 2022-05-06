using System.Reflection;
using System.Runtime.InteropServices;
using static RegionKit.RegionKitMod;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Region-Kit")]
[assembly: AssemblyDescription("Region Development Kit for Rain World")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Substratum Dev Team & More")]
[assembly: AssemblyProduct("Region-Kit")]
[assembly: AssemblyCopyright("Copyright © 2021")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("a8d4f157-97e1-4cb6-9f79-7eda989ec893")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
#warning are you pushing an update? update assembly version here VVV
[assembly: AssemblyVersion(modVersion + "." + buildVersion)]
[assembly: AssemblyFileVersion(modVersion + "." + buildVersion)]
[assembly: AssemblyInformationalVersion(modVersion)]