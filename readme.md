# BribeForLocation

Ever wish you could bribe an NPC to just mark a location on your map instead of clicking on it a bunch and hoping they do? Me too, so I made this mod for [Daggerfall Unity](https://github.com/Interkarma/daggerfall-unity/).

## Development Status

Early devlopment. I have not taken compatibility with existing mods into mind just yet.

## Settings

### General

#### Base Bribe Amount
The starting amount required for a bribe before scaling.
*Default Value*: 10
_Min_: 1
_Max_: 100

#### Enable Scale By level
When enabled, the amount required for bribes scales with the players level.
*Default Value*: true

#### Level Scale Amount
How drastically to scale the bribe amount per level.
*Default Value*: 0.5
_Min_: 0
_Max_: 4

#### Enable Scale By Personality
Reduce or Increase bribe amounts based on the Personality attribute.
*Default Value*: true

#### Personality Scale Amount
Tune how Personality affects bribes. \[0.5 = 100 Personality bribes amounts are halved\] \[2 = 1 Personality doubles the amount\].
*Default Value*:
```
{
    First: 0.5,
    Second: 2
}
```
_Min_: 0
_Max_: 10

### Additional Fees
Add a premium for specialized information.

#### People Fee
When asking NPCs about specific people.
*Default Value*: 20
_Min_: 0
_Max_: 200

#### Work Fee
When asking NPCs about any work.
*Default Value*: 10
_Min_: 0
_Max_: 100

### Experimental

#### Enable Knowledge Check
Enables native NPC knowledge using dotnet reflection to parts of the games code that wouldn't normally be reusable. Well supported on Windows but may not work on all platforms like Android.
*Default Value*: false