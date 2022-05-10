# Region Kit

A unified dependency and object kit for Rain World region modding. Created by The Substratum Dev Team and other members of the community.

#### Table of Contents
- [The Goal](#the-goal)
- [Features](#features)
- [Credits](#credits)
- [Download](#download)
- [More Info](#more-info)


## The Goal

Our goal for region kit is to have a unified mod to hold objects, room effects, and region infrastructure, so that region creators only need to look for one place rather than worry about the need to use another region's code mod. 


## Features

- Superstructure Fuses Fix
  	- Fixes Superstructure fuses in arena mode, preventing crashes.
- Arena Management
  	- Text file that can be placed inside a subfolder in the levels folder to manage custom arenas.
- PWLightRod
  	- RGB based SSLightRod.
- Placed Objects Manager Framework
  	- Framework created by Henpemaz to allow for easy creation of custom objects.
- Placed Wormgrass
  	- PlacedObject for setting up wormgrass without re-exporting room geometry.
- RegionKit.Machinery
  	- A small set of customizable objects for adding moving parts like cogs and pistons to your levels. Can use any loaded sprites and shaders by name.
  	- NOTE: MachineryCustomizer object is used for changing sprite/container/shader settings.
  	- A general purpose power level customization system, a related interface to be used. See code: `RegionKit.Machinery.RoomPowerManager`, `RegionKit.Machinery.RoomPowerManager.IRoomPowerModifier`.
- Spinning Fan
  	- Animated and scalable fan.
- Echo Extender
	- Allows adding echoes to custom regions without any coding.
	- For usage see [this file.](EchoExtender.md)
- The Mast
	- Implements all of the contents that went to the mast.
	- Adds custom wind object.
	- Adds custom pearl chain object.
	- Misc additions solely for The Mast region.
- ARObjects
	- Implements contents from Aether Ridge
	- Adds a rectangle object that kills the player when entered.
- Flooded Assembly Objects
	- Ported content out of an uponcoming regionpack
	- Adds more advanced variations of CustomDecal (FreeformDecalOrSprite) and LightSource (ColouredLightSource)
- Arid Barrens
	- Adds Sandstorm Effect
- ShelterBehaviors
	- Adds placed objects to control and customize how your shelters act
	(standalone version will take priority if present)
- ExtendedGates
	- Enables you to set additional gate pass requirements: karma above 5, mark of communication etc
	- Features a set of new holograms
	(for instructions see https://github.com/henpemaz/PartModPartMeme/blob/master/ExtendedGates/Resources/README.txt)
	(standalone version will take priority if present)
- CustomSpritesLoader
	- Allows easily loading in atlases or separate sprites without additional code (as a region maker, put things into `[regpack]/Assets/Load/`)
	(standalone version will take priority if needed)
  
### Particle system

 RegionKit provides a general purpose particle system, featuring:
  - Use of arbitrary sprites
  - Controlled randomization of visuals and movement
  - Modularity: combine visuals, behaviour and modifiers in any way you like.
 Expandability:
	- Relatively simple to make user defined behaviours
	- Basework for making new types of emitters, particle classes and behaviour modifier classes
For more detailed instructions, see: `P_GUIDE.md`

### CustomSpritesLoader
RegionKit packs an instance of CSL by Henpemaz. It allows the user to easily load arbitrary sprites.
Region makers can place atlases into `[regpack]/Assets/Load/` directory to easily access new sprites for things like particle systems.

## Credits

- DryCryCrystal 
	- Manager and Dev Team Lead
	- Porting

- Thalber
	- BepInEx migration
	- RegionKit.Machinery
	- Particle Systems
	- Misc internal janitoring

- DeltaTime
	- Initial versions
	- PWLightRod
	- PWMalfunction
	- Arena Management

- M4rbleL1ne
	- Several arena fixes
	- Customisable Effect Colors
	- NoWallSlideZones
	- GlowingSwimmers
	- SunblockerFix

- Henpemaz
	- Placed Objects Manager Framework
	- CustomSpritesLoader
	- ShelterBehaviors

- Thrithralas
	- Echo Extender
	- Flooded Assembly Ported Objects (ColouredLightSource, FreeformDecalOrSprite)
	- Vector2ArrayField for POM Framework

- Slime_Cubed
	- Superstructure Fuses Fix
	- The Mast
	- Conditional Effects
	- Fog of War Effects (Line of Sight)

- LeeMoriya
	- ARObjects
	- Spinning Fan

- Doggo
	- The Mast Permission

- Kaeporo
	- ARObjects Permission

- Dracentis
	- Arid Barrens Code

**Initial ExtendedGates authors**: Henpemaz (code); Mehri'Kairotep, Thalber, LB/M4rbleL1ne and Nautillo (spritework).

## Download
Downloads can be found [here](https://github.com/DryCryCrystal/Region-Kit/releases/latest).

**Requires EnumExtender and Custom Regions Support**

Region Kit also comes packed with auto update support.


## More Info

More information such as how to make use of some of the features can be found on the [Modding Wiki](https://rain-world-modding.github.io/).
