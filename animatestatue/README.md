This is a mod for Caves of Qud that makes statues animatable by all standard means (Spray-a-Brain, nano-neuro animators).
Dynamic statues of creatures have the anatomy of their creature type, while predefined statues have the best anatomy I
can assign based on their appearances and descriptions. The easiest way to install it is from [the Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=2918604974).

Specifically, this involves:

1. Changing the BodyType check when animating objects from a GetTag to a GetTagOrStringProperty call, which
  allows body types for animatable objects to be set at runtime. This is a small change but requires a fiddly Harmony transpiler 
  patch and may break in the future.
2. Support for a BodyCategory tag/property, which is modeled after the BodyType tag and causes all body parts to be set to 
  that category (e.g. Stone, Metal, Mechanical; always Stone currently) when animated.
3. BodyType is set at runtime for dynamic statues to the anatomy of the creature depicted.
4. All statue blueprints are made Animatable, their BodyCategories Stone, and their BodyTypes appropriate for their appearance 
  in the case of non-dynamic statues.
5. A couple new anatomies for statues that don't have appropriate existing anatomies (six-armed humanoid for Oboroqoru, eroded for
  implanted Eater statues).


Known limitations:

* There's no way to track mutations on statues, so they won't show up on animated statues even if logically they'd be part of it (e.g. multiple arms, chimera limbs, wings).
* Since sultan shrines and Eater statues have both humanoid and non-humanoid variants that are identical except for their render tiles, they're always humanoid when animated, since there's no clean way to set the anatomies differently.

This mod is *probably* compatible with any mod which doesn't change statues or how animating objects works. But it involves a rather fiddly Harmony patch, so no guarantees.
