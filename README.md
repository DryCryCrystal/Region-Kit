# Region Kit

A unified dependency and object kit for Rain World region modding. Created by The Substratum Dev Team and other members of the community.

#### Table of Contents
- [The Goal](https://github.com/DryCryCrystal/Region-Kit#the-goal)
- [Features](https://github.com/DryCryCrystal/Region-Kit#features)
- [Credits](https://github.com/DryCryCrystal/Region-Kit#credits)
- [Download](https://github.com/DryCryCrystal/Region-Kit#download)
- [More Info](https://github.com/DryCryCrystal/Region-Kit#more-info)


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
- Placed wormgrass
  - PlacedObject for setting up wormgrass without re-exporting room geometry.
- RegionKit.Machinery
  - A small set of customizable objects for adding moving parts like cogs and pistons to your levels. Can use any loaded sprites and shaders by name.
  - NOTE: MachineryCustomizer object is used for changing sprite/container/shader settings.
  - A general purpose power level customization system, a related interface to be used. See code: `RegionKit.Machinery.RoomPowerManager`, `RegionKit.Machinery.RoomPowerManager.IRoomPowerModifier`.
- Echo Extender
	- Allows adding echoes to custom regions without any coding .
- The Mast
	- Implements all of the contents that went to the mast.
	- Adds custom wind object.
	- Adds custom pearl chain object.
	- Misc additions solely for The Mast region.
- ARObjects
	- Implements contents from Aether Ridge
	- Adds a rectangle object that kills the pl.ayer when entered.
  
### Particle system

 RegionKit provides a general purpose particle system, featuring:
  - Use of arbitrary sprites
  - Controlled randomization of visuals and movement
  - Modularity: combine visuals, behaviour and modifiers in any way you like.
 Expandability:
	- Relatively simple to make user defined behaviours
	- Basework for making new types of emitters, particle classes and behaviour modifier classes
For more detailed instructions, see: `P_GUIDE.md`


## Credits

- DryCryCrystal 
	- Manager and Dev Team Lead
	- Porting

- DeltaEcho
	- Initial versions
	- PWLightRod
	- PWMalfunction
	- Arena Management

- Henpemaz
	- Placed Objects Manager Framework

- Thalber
	- RegionKit.Machinery
	- Particle System

- Thriftralas
	- Echo Extender

- Slime_Cubed
	- Superstructure Fuses Fix
	- The Mast

- Doggo
	- The Mast

- LeeMoriya
	- ARObjects


## Download
Downloads can be found [here](https://github.com/DryCryCrystal/Region-Kit/releases).

**Requires EnumExtender**

Region Kit also comes packed with auto update support.


## More Info

More information such as how to make use of some of the features can be found on the [Modding Wiki](https://rain-world-modding.github.io/).
