# csChat

Minimalist Client/Server TextRPG and Chat.

!help
for commands when connected.

## Notes 
    csClient
    - potential race condition when receving/sending message at the same time (not an issue at the moment, but keep in mind for potential more complex clien-server interactions)
      solution: saving received messages in Queue and have it emptied by IOHandler.Run().

## Issues
    - level up mechanic missing when completing a quest

## To-Do
    - Quest generator (low prio)
    - Location Generator (low prio)
    - reload Quests from Json file at request
    - fight Local Enemies
    - implement new itemDB compatible with WorldEditor
    - add quest items
    - add a location selection feature to the "move" command in the world editor when editing or creating a quest, right now you have to manually type the location name
    - Create "Server Manager" project that allows to create, edit, and delete users
    - Add the feature to blacklist and disconnect users to the "server2" project

## Attributes
- Speed:        decides who strikes first, is the "attack value".
- Intellect:    is the "defend value", is the prerequisite to finding quests.
- Luck:         is the "crit chance", if a crit occurs the damage is multiplied by 2, it influeces the probability of a loot drop.
                The crit probability and loot drop probability depends on the players luck attribute and the enemys luck attribute if both values are equal base chance applies.
                The probability scales logarithmic (500 luck = 99% crit chance, 10 luck = 9,52% crit chance) and the enemy luck value needs to be approximately 69.3 higher than the player's luck to halve the critical hit chance