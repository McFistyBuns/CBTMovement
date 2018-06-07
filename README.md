# CBT Movement

CBT Movement is an attempt to bring Classic Battletech Tabletop movement rules flavor into HBS's BATTLETECH game.  Features include:

- Sprinting no longer ends the turn
- Evasion is no longer removed after attacks
- Any movement now incurs a +1 ToHit Penalty
- Sprinting incurs an addtional +1 ToHit Penalty
- Jumping incures an additional +2 ToHit Penalty
- ToHit modifiers are allowed to go below your base to hit chance, making something easier to hit if you stack you modifiers right

The way movement currently works in the game is that ToHitSelfWalk modifiers are applied whenever you make any movement.  So Sprinting, for example, will have a +1 for movement and an additional +1 for sprinting, bringing
it in line with the original Tabletop rules of +2.  The same applies to the Jump ToHit Modifiers.

## Installation

Install [BTML](https://github.com/Mpstark/BattleTechModLoader) and [ModTek](https://github.com/Mpstark/ModTek). Extract files to `BATTLETECH\Mods\CBTMovement\`.

## Configuration

`mod.json` contains movement ToHit modifier for Jumping.  Walk and Sprint movement ToHit modifiers are found in the StreamingAssests\data\constants\CombatGameConstants.json file
