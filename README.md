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
- Private message does not work anymore pls fix !!!

## To-Do
- Quest generator (low prio)
- Location Generator (low prio)
- reload Quests from Json file at request ( so that quest can be updated and changed regardless of the server running and users questing)
- check if Quest Enemies are created correctly and work correctly in game (high prio)
- implement new itemDB compatible with WorldEditor
- test items rework
- add inventory size / item limit
- add quest items
- add "Escalations" (Escalations are quests that have a rare chance of appearing when killing and enemy or completing a specific quest, they are triggerd by using quest items in specific loactions)
- add a location selection feature to the "move" command (if locations are connected, right now "ethical" moving is only possible trough quest)
- add a location selection feature to the "move" command in the world editor when editing or creating a quest, right now you have to manually type the location name
- Add the feature to blacklist and disconnect users to the "server2" project
- Worldeditor: test if shops are created properly
- test if creating and editing quests still works properly
- fix PM (Private Message) feature
- add an "accept duel" feature
- add a party feature
- add player trading 
- add player market
- expand Item crafting/modification (add probability depending on the intellect attribute)


## Attributes
- Speed:        decides who strikes first, is the "attack value".
- Intellect:    is the "defend value", is the prerequisite to finding quests.
- Luck:         is the "crit chance", if a crit occurs the damage is multiplied by 2, it influeces the probability of a loot drop.
                The crit probability and loot drop probability depends on the players luck attribute and the enemys luck attribute if both values are equal base chance applies.
                The probability scales logarithmic (500 luck = 99% crit chance, 10 luck = 9,52% crit chance) and the enemy luck value needs to be approximately 69.3 higher than the player's luck to halve the critical hit chance

