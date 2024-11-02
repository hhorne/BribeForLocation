# BribeForLocation

Ever wish you could bribe an NPC to just mark a location on your map instead of clicking on it a bunch and hoping they do? Me too, so I made this mod for [Daggerfall Unity](https://github.com/Interkarma/daggerfall-unity/).

## Development Status

Early devlopment. I have not taken compatibility with existing mods into mind just yet.

## Settings

### General

#### StartingBribeAmount
The  amount required for a bribe before scaling.
*Default Value*: 10
_Min_: 1
_Max_: 100

#### EnableScaleBylevel
When enabled, the amount required for bribes scales with the players level.
*Default Value*: true

#### LevelScaleAmount
How drastically to scale the bribe amount per level.
*Default Value*: 0.5
_Min_: 0
_Max_: 4

#### EnableScaleByPersonality
Reduce or Increase bribe amounts based on the Personality attribute.
*Default Value*: true

#### PersonalityScaleAmount
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

### AdditionalFees
Add a premium for specialized information.

#### PeopleFee
When asking NPCs about specific people.
*Default Value*: 20
_Min_: 0
_Max_: 200

#### WorkFee
When asking NPCs about any work.
*Default Value*: 10
_Min_: 0
_Max_: 100

### Experimental

#### EnableKnowledgeCheck
Enables native NPC knowledge using dotnet reflection to parts of the games code that wouldn't normally be reusable. Well supported on Windows but may not work on all platforms like Android.
*Default Value*: false