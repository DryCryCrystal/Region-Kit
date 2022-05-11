== GENERAL ==
- Build refs go to `libs/` inside project folder; they are gitignored. Get valid refs with https://github.com/Reinms/Stubber-Publicizer.
- Main plugin class: `RegionKitMod.cs`. Please avoid clutter there and only add initialization calls; add your stuff to separate classes and/or sub-namespaces.
- `Utils/` has some boilerplate methods for reflection / unity primitives conversion etc. Take a peek there if you feel like you're living a ground hog day (or if you'd want to save someone from that feeling in the future)
- Use Utils/PetrifiedWood for logging. Use class name aliases if you want it to be shorter (`using PWood = RegionKit.Utils.PetrifiedWood;`). When importing parts, move their logging there too.

== MAINTAIN CSPROJ HYGIENE! ==
Keep modifications to a minimum. If you are adding a subnamespace, make sure csproj is not flooded with entries for every single code file. Use format `<Compile Include="Utils\*.cs" />`.
Do the same for Embedded Resouces (ER) and anything else that affects csproj.
ER tend to split into separate PropertyGroups for some reason; please fix if that happens with yours.