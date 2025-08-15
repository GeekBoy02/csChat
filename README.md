# csChat
## Available Commands

- `!class [class_name]`: Sets the user's class to the specified class name. Valid class names are: Soldier, Engineer, Explorer.

- `!inventory`: Displays your inventory, showing all the items you currently possess.
  `!use [item name]`: Use an item from your inventory by specifying its name.   
- `!i [item name]`: Use an item from your inventory by specifying its name.
- `!ir [item name]`: Remove an item from your inventory by specifying its name.
- `!ii [item name]`: Inspect an item in your inventory to get more information about it.
- `!is [item name]`: Sell an item from your inventory to earn in-game currency.
- `!isa [item name]`: Sell all items of a specific name from your inventory.

- `!shop`: Displays the local shop's inventory if there is one available.
- `!shop [item name]`: Buy a specific item from the shop by providing its name.

- `!revive`: Revives your character, allowing you to continue playing.

- `!fight [enemy_level]`: Initiates a battle with an enemy of the specified level.

- `!duel [username]`: Initiates a battle with another online user, provided their username.

- `!allocate_attributes [speed] [intellect] [luck]`: Increases your character's speed, intellect, and luck attributes by the specified amounts.

- `!attributes`: Displays your character's current attributes, including speed, intellect, and luck.

- `!users`: Displays a list of all connected users.

- `!local`: Displays a list of all nearby users in the game.

- `!quest`: Progress your Active Quest.
- `!qg [Questname]`: Display/Accept a local Quest.

- `![username] [message]`: Send a private message to a specific user by mentioning their username.



## Problems
    -

## To-Do
    - Quest generator
    - Location Generator
    - reload Quests from Json file at request
    - fight Local Enemies 
    - fix using multiple items with !i and !use

## Attributes
- Speed:        decides who strikes first, is the "attack value"
- Intellect:    is the "defend value", is the prerequisite to finding quests
- Luck:         is the "crit chance", if a crit occurs the damage is multiplied by 2, influeces the probability of a loot drop
                the crit probability and loot drop probability depends on the players luck attribute and the enemys luck attribute if both values are equal base chance applies.
                the probability scales logarithmic (500 luck = 99% crit chance, 10 luck = 9,52% crit chance) and the enemy luck value needs to be approximately 69.3 higher than the player's luck to halve the critical hit chance