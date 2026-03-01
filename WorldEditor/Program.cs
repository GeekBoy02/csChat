using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace WorldEditor
{
    class Program
    {
        static JsonSerializerOptions jopts = new JsonSerializerOptions { WriteIndented = true };

        static void Main()
        {
            var exeDir = AppContext.BaseDirectory;
            var projectRoot = Path.GetFullPath(Path.Combine(exeDir, "..", "..", "..", ".."));
            var worldDir = Path.Combine(projectRoot, "WorldEditor", "world");
            Directory.CreateDirectory(worldDir);
            Console.WriteLine("WorldEditor - simple console tool to create/list/edit world JSON files");

            // Main menu loop
            for (; ; )
            {
                Console.WriteLine();
                Console.WriteLine("Main menu - choose where to work:");
                Console.WriteLine("  1) World editor");
                Console.WriteLine("  2) ItemDB editor");
                Console.WriteLine("  3) Quest editor");
                Console.WriteLine("  help | ?      - show this menu");
                Console.WriteLine("  exit          - quit");
                Console.Write("> ");
                var sel = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(sel)) continue;
                sel = sel.Trim();
                if (sel.Equals("1") || sel.Equals("world", StringComparison.OrdinalIgnoreCase))
                {
                    // enters world-focused command loop; returns true if user requested exit
                    var exitRequested = WorldMenu(worldDir);
                    if (exitRequested) break;
                    continue;
                }
                if (sel.Equals("2") || sel.Equals("itemdb", StringComparison.OrdinalIgnoreCase))
                {
                    var exitRequested = ItemDbMenu(worldDir);
                    if (exitRequested) break;
                    continue;
                }
                if (sel.Equals("3") || sel.Equals("quests", StringComparison.OrdinalIgnoreCase))
                {
                    var exitRequested = QuestMenu(worldDir);
                    if (exitRequested) break;
                    continue;
                }
                if (sel.Equals("help", StringComparison.OrdinalIgnoreCase) || sel.Equals("?"))
                {
                    PrintHelpMain(worldDir);
                    continue;
                }
                if (sel.Equals("exit", StringComparison.OrdinalIgnoreCase) || sel.Equals("4")) break;
                Console.WriteLine("Unknown selection, type 'help' or '?' to see options.");
            }
        }

        // Returns true if user requested exit the whole application
        static bool WorldMenu(string worldDir)
        {
            for (; ; )
            {
                Console.WriteLine();
                Console.WriteLine("World commands: list | new | edit <file> | itemdb | help | ? | back | exit");
                Console.Write("> ");
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                var cmd = parts[0].ToLowerInvariant();

                if (cmd == "back") return false;
                if (cmd == "exit") return true;

                if (cmd == "list")
                {
                    var files = GetWorldFiles(worldDir);
                    if (files.Count == 0) Console.WriteLine("No world files found.");
                    for (int i = 0; i < files.Count; i++) Console.WriteLine(files[i].display);
                    continue;
                }

                if (cmd == "new")
                {
                    CreateNewLocation(worldDir);
                    continue;
                }
                if (cmd == "edit")
                {
                    if (parts.Length < 2)
                    {
                        var files = GetWorldFiles(worldDir);
                        if (files.Count == 0) { Console.WriteLine("No world files available to edit."); continue; }
                        Console.WriteLine("Select a file to edit:");
                        for (int i = 0; i < files.Count; i++) Console.WriteLine($"  [{i + 1}] {files[i].display}");
                        Console.Write("> ");
                        var sel = Console.ReadLine();
                        if (!int.TryParse(sel, out var idx) || idx < 1 || idx > files.Count) { Console.WriteLine("Invalid selection"); continue; }
                        var resolved = files[idx - 1].path;
                        EditLocation(resolved, worldDir);
                        continue;
                    }
                    var input = parts[1].Trim();
                    var resolved2 = ResolveWorldPath(worldDir, input);
                    if (resolved2 == null) { Console.WriteLine("file not found: " + input); continue; }
                    EditLocation(resolved2, worldDir);
                    continue;
                }

                if (cmd == "help" || cmd == "?")
                {
                    PrintHelpWorld(worldDir);
                    continue;
                }

                if (cmd == "itemdb")
                {
                    var exitRequested = ItemDbMenu(worldDir);
                    if (exitRequested) return true;
                    continue;
                }

                Console.WriteLine("Unknown command");
            }
        }

        // Returns true if user requested exit the whole application
        static bool ItemDbMenu(string worldDir)
        {
            var dbPath = Path.Combine(worldDir, "ItemDB", "items.json");
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath) ?? "");
            for (; ; )
            {
                Console.WriteLine();
                var dbInfo = LoadItemDb(dbPath);
                Console.WriteLine($"ItemDB editor - commands: list | add | populate | remove <index> | edit <index> | back | exit  (path: {dbPath}, items: {dbInfo.Count})");
                Console.Write("> ");
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                var action = parts[0].ToLowerInvariant();
                if (action == "back") return false;
                if (action == "exit") return true;

                var items = LoadItemDb(dbPath);
                if (action == "list")
                {
                    PrintItemDb(items);
                }
                else if (action == "add")
                {
                    do
                    {
                        var it = new Item
                        {
                            name = Prompt("item name", "Item"),
                            icon = Prompt("icon", ""),
                            description = Prompt("description", ""),
                            value = PromptInt("value", 0),
                        };
                        var existsIndex = items.FindIndex(x => string.Equals(x.name, it.name, StringComparison.OrdinalIgnoreCase));
                        if (existsIndex >= 0) Console.WriteLine($"Item '{it.name}' already exists in ItemDB at index {existsIndex}. Use 'edit {existsIndex}' to modify it. Skipping add.");
                        else { items.Add(it); SaveItemDb(dbPath, items); }
                    }
                    while (Confirm("Add another item? (y/N): "));
                }
                else if (action == "populate")
                {
                    var defaults = new List<Item>
                {
                    new Item { name = "Bandage", icon = "🩹", description = "Restores some health.", value = 1 },
                    new Item { name = "Drink", icon = "🧃", description = "Tasty Drink that restores some health.", value = 2 },
                    new Item { name = "Boots", icon = "👢", description = "Increases your SPEED in a Fight", value = 10 },
                    new Item { name = "Glasses", icon = "👓", description = "Increases your INTELLECT in a Fight", value = 10 },
                    new Item { name = "Scanner", icon = "📡", description = "Increases your LUCK in a Fight", value = 10 }
                };
                    var added = 0;
                    foreach (var d in defaults)
                    {
                        if (!items.Exists(x => string.Equals(x.name, d.name, StringComparison.OrdinalIgnoreCase)))
                        {
                            items.Add(d);
                            added++;
                        }
                    }
                    if (added > 0) SaveItemDb(dbPath, items);
                    Console.WriteLine($"Populated ItemDB with {added} new items.");
                }
                else if (action == "remove" && parts.Length > 1 && int.TryParse(parts[1], out var ridx) && ridx >= 0 && ridx < items.Count)
                {
                    items.RemoveAt(ridx);
                    SaveItemDb(dbPath, items);
                }
                else if (action == "edit" && parts.Length > 1 && int.TryParse(parts[1], out var eidx) && eidx >= 0 && eidx < items.Count)
                {
                    var it = items[eidx];
                    it.name = Prompt("new name (enter to keep)", it.name);
                    it.icon = Prompt("new icon (enter to keep)", it.icon);
                    it.description = Prompt("new description (enter to keep)", it.description);
                    it.value = PromptInt("new value (enter to keep)", it.value);
                    items[eidx] = it; SaveItemDb(dbPath, items);
                }
                else
                {
                    Console.WriteLine("Usage: [list|add|populate|remove <index>|edit <index>|back|exit]");
                }
            }
        }

        static void CreateNewLocation(string worldDir)
        {
            Console.Write("name: "); var name = Console.ReadLine() ?? "New-Location";
            Console.Write("level (int): "); var lvlStr = Console.ReadLine(); int.TryParse(lvlStr, out var level);
            Console.Write("description: "); var desc = Console.ReadLine() ?? "";
            Console.Write("welcomeMsg: "); var welcome = Console.ReadLine() ?? "";

            var loc = new Location { name = name, level = level, description = desc, welcomeMsg = welcome, x = 0, y = 0 };

            // create a folder per location and write the JSON inside it
            var locationDir = Path.Combine(worldDir, name);
            Directory.CreateDirectory(locationDir);
            var filename = Path.Combine(locationDir, name + ".json");
            var json = JsonSerializer.Serialize(loc, jopts);
            File.WriteAllText(filename, json);
            Console.WriteLine("Created: " + Path.GetFullPath(filename));
        }

        static void EditLocation(string path, string worldDir)
        {
            var text = File.ReadAllText(path);
            var loc = JsonSerializer.Deserialize<Location>(text) ?? new Location();
            Console.WriteLine("current name: " + loc.name);
            Console.Write("new name (enter to keep): "); var name = Console.ReadLine(); if (!string.IsNullOrWhiteSpace(name)) loc.name = name;
            Console.WriteLine("current level: " + loc.level);
            Console.Write("new level (enter to keep): "); var lvl = Console.ReadLine(); if (int.TryParse(lvl, out var nl)) loc.level = nl;
            Console.WriteLine("current description: " + loc.description);
            Console.Write("new description (enter to keep): "); var desc = Console.ReadLine(); if (!string.IsNullOrWhiteSpace(desc)) loc.description = desc;
            Console.WriteLine("current welcomeMsg: " + loc.welcomeMsg);
            Console.Write("new welcomeMsg (enter to keep): "); var wm = Console.ReadLine(); if (!string.IsNullOrWhiteSpace(wm)) loc.welcomeMsg = wm;
            // allow editing enemies (show loot counts)
            Console.WriteLine($"Enemies ({loc.enemies.Count}):");
            for (int i = 0; i < loc.enemies.Count; i++)
            {
                var e = loc.enemies[i];
                var lootCount = e.loot?.Count ?? 0;
                Console.WriteLine($"  [{i}] {e.name} Lv{e.Level} (loot: {lootCount})");
            }
            Console.WriteLine("Commands for enemies: add | remove <index> | loot <index> | skip (type 'help' for details)");
            Console.Write("enemies> "); var ecmd = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(ecmd))
            {
                if (ecmd.Trim().Equals("help", StringComparison.OrdinalIgnoreCase) || ecmd.Trim().Equals("?", StringComparison.OrdinalIgnoreCase))
                {
                    PrintHelpEnemies(worldDir);
                    ecmd = null; // treat as no-op for main flow
                }
                var ep = ecmd.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
                if (ep[0] == "add")
                {
                    while (true)
                    {
                        var en = new Enemy();
                        Console.Write("enemy name: "); en.name = Console.ReadLine() ?? "Enemy";
                        Console.Write("Level: "); if (int.TryParse(Console.ReadLine(), out var tmpLevel)) en.Level = tmpLevel;
                        Console.Write("hp: "); if (int.TryParse(Console.ReadLine(), out var tmpHp)) en.hp = tmpHp;
                        Console.Write("speed: "); if (int.TryParse(Console.ReadLine(), out var tmpSpeed)) en.speed = tmpSpeed;
                        Console.Write("int: "); if (int.TryParse(Console.ReadLine(), out var tmpInt)) en.intellect = tmpInt;
                        Console.Write("luck: "); if (int.TryParse(Console.ReadLine(), out var tmpLuck)) en.luck = tmpLuck;
                        Console.Write("credits: "); if (int.TryParse(Console.ReadLine(), out var tmpCredits)) en.credits = tmpCredits;
                        loc.enemies.Add(en);
                        Console.Write("Add another enemy? (y/N): "); var more = Console.ReadLine();
                        if (string.IsNullOrWhiteSpace(more) || !more.Trim().Equals("y", StringComparison.OrdinalIgnoreCase)) break;
                    }
                }
                else if (ep[0] == "remove" && ep.Length > 1 && int.TryParse(ep[1], out var ridx) && ridx >= 0 && ridx < loc.enemies.Count)
                {
                    loc.enemies.RemoveAt(ridx);
                }
                else if (ep[0] == "loot" && ep.Length > 1 && int.TryParse(ep[1], out var lidx) && lidx >= 0 && lidx < loc.enemies.Count)
                {
                    // manage loot for a specific enemy
                    var enemy = loc.enemies[lidx];
                    Console.WriteLine($"Loot for {enemy.name} (count: {enemy.loot?.Count ?? 0}):");
                    for (int li = 0; li < (enemy.loot?.Count ?? 0); li++) Console.WriteLine($"  [{li}] {enemy.loot[li].name} ({enemy.loot[li].value})");
                    Console.WriteLine("Commands: add | remove <index> | skip (type 'help' for details)");
                    Console.Write($"loot[{lidx}]> "); var lcmd = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(lcmd))
                    {
                        if (lcmd.Trim().Equals("help", StringComparison.OrdinalIgnoreCase) || lcmd.Trim().Equals("?", StringComparison.OrdinalIgnoreCase))
                        {
                            PrintHelpLoot(worldDir, enemy.name);
                            lcmd = null;
                        }
                        var lp = lcmd.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                        if (lp[0] == "add")
                        {
                            // select items from ItemDB only
                            var dbPath = Path.Combine(worldDir, "ItemDB", "items.json");
                            var db = LoadItemDb(dbPath);
                            if (db.Count == 0) { Console.WriteLine("ItemDB is empty. Use `itemdb add` to populate it."); }
                            else
                            {
                                while (true)
                                {
                                    for (int di = 0; di < db.Count; di++) Console.WriteLine($"  [{di}] {db[di].name} ({db[di].value})");
                                    Console.Write("pick item index to add (or blank to cancel): "); var pick = Console.ReadLine();
                                    if (string.IsNullOrWhiteSpace(pick)) break;
                                    if (!int.TryParse(pick, out var pidx) || pidx < 0 || pidx >= db.Count) { Console.WriteLine("Invalid index"); continue; }
                                    enemy.loot ??= new List<Item>();
                                    // clone selected item
                                    var chosen = db[pidx];
                                    enemy.loot.Add(new Item { name = chosen.name, icon = chosen.icon, description = chosen.description, value = chosen.value });
                                    Console.Write("Add another loot item? (y/N): "); var moreI = Console.ReadLine();
                                    if (string.IsNullOrWhiteSpace(moreI) || !moreI.Trim().Equals("y", StringComparison.OrdinalIgnoreCase)) break;
                                }
                            }
                        }
                        else if (lp[0] == "remove" && lp.Length > 1 && int.TryParse(lp[1], out var rli) && rli >= 0 && rli < (enemy.loot?.Count ?? 0))
                        {
                            enemy.loot.RemoveAt(rli);
                        }
                    }
                }
            }

            // allow editing shop items
            Console.WriteLine($"Shop ({loc.shop.Count} items):");
            for (int i = 0; i < loc.shop.Count; i++) Console.WriteLine($"  [{i}] {loc.shop[i].name} ({loc.shop[i].value})");
            Console.WriteLine("Commands for shop: add | remove <index> | skip (type 'help' for details)");
            Console.Write("shop> "); var scmd = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(scmd))
            {
                if (scmd.Trim().Equals("help", StringComparison.OrdinalIgnoreCase) || scmd.Trim().Equals("?", StringComparison.OrdinalIgnoreCase))
                {
                    PrintHelpShop(worldDir);
                    scmd = null;
                }
                var sp = scmd.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                if (sp[0] == "add")
                {
                    // add items only from ItemDB
                    var dbPath = Path.Combine(worldDir, "ItemDB", "items.json");
                    var db = LoadItemDb(dbPath);
                    if (db.Count == 0) { Console.WriteLine("ItemDB is empty. Use `itemdb add` to populate it."); }
                    else
                    {
                        while (true)
                        {
                            for (int di = 0; di < db.Count; di++) Console.WriteLine($"  [{di}] {db[di].name} ({db[di].value})");
                            Console.Write("pick item index to add (or blank to cancel): "); var pick = Console.ReadLine();
                            if (string.IsNullOrWhiteSpace(pick)) break;
                            if (!int.TryParse(pick, out var pidx) || pidx < 0 || pidx >= db.Count) { Console.WriteLine("Invalid index"); continue; }
                            var chosen = db[pidx];
                            loc.shop.Add(new Item { name = chosen.name, icon = chosen.icon, description = chosen.description, value = chosen.value });
                            Console.Write("Add another item? (y/N): "); var moreI = Console.ReadLine();
                            if (string.IsNullOrWhiteSpace(moreI) || !moreI.Trim().Equals("y", StringComparison.OrdinalIgnoreCase)) break;
                        }
                    }
                }
                else if (sp[0] == "remove" && sp.Length > 1 && int.TryParse(sp[1], out var ridx2) && ridx2 >= 0 && ridx2 < loc.shop.Count)
                {
                    loc.shop.RemoveAt(ridx2);
                }
            }

            // simple name validation and move file if name changed
            var oldPath = path;
            var newName = loc.name;
            if (!ValidateName(newName))
            {
                Console.WriteLine("Invalid name after edit (contains invalid path characters). Save aborted.");
                return;
            }

            // write back
            File.WriteAllText(path, JsonSerializer.Serialize(loc, jopts));
            Console.WriteLine("Saved: " + Path.GetFileName(path));

            // if name changed and file is inside a folder named after old name, move it
            var parent = Path.GetDirectoryName(path) ?? "";
            var oldFileName = Path.GetFileName(path);
            if (!string.Equals(Path.GetFileNameWithoutExtension(oldFileName), newName, StringComparison.Ordinal))
            {
                var newDir = Path.Combine(Path.GetDirectoryName(parent) ?? parent, newName);
                Directory.CreateDirectory(newDir);
                var newPath = Path.Combine(newDir, newName + ".json");
                File.Move(path, newPath, overwrite: true);
                // remove old dir if empty
                try { if (Directory.Exists(parent) && Directory.GetFiles(parent).Length == 0 && Directory.GetDirectories(parent).Length == 0) Directory.Delete(parent); } catch { }
                Console.WriteLine("Moved file to: " + newPath);
            }
        }

        static bool ValidateName(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars()) if (name.Contains(c)) return false;
            return !string.IsNullOrWhiteSpace(name);
        }

        static List<(string display, string path)> GetWorldFiles(string worldDir)
        {
            var result = new List<(string, string)>();
            foreach (var f in Directory.GetFiles(worldDir, "*.json")) result.Add((Path.GetFileName(f), f));
            foreach (var d in Directory.GetDirectories(worldDir))
            {
                var dirName = Path.GetFileName(d);
                foreach (var f in Directory.GetFiles(d, "*.json")) result.Add(($"{dirName}{Path.DirectorySeparatorChar}{Path.GetFileName(f)}", f));
            }
            return result;
        }

        static List<Item> LoadItemDb(string path)
        {
            try
            {
                if (!File.Exists(path)) return new List<Item>();
                var txt = File.ReadAllText(path);
                return JsonSerializer.Deserialize<List<Item>>(txt) ?? new List<Item>();
            }
            catch
            {
                return new List<Item>();
            }
        }

        static void SaveItemDb(string path, List<Item> items)
        {
            try
            {
                var dir = Path.GetDirectoryName(path) ?? ".";
                Directory.CreateDirectory(dir);
                File.WriteAllText(path, JsonSerializer.Serialize(items, jopts));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to save ItemDB: " + ex.Message);
            }
        }

        static void PrintItemDb(List<Item> items)
        {
            if (items.Count == 0) { Console.WriteLine("ItemDB empty"); return; }
            for (int i = 0; i < items.Count; i++) Console.WriteLine($"[{i}] {items[i].name} ({items[i].value})");
        }

        static string Prompt(string label, string @default)
        {
            Console.Write(label + ": ");
            var v = Console.ReadLine();
            return string.IsNullOrWhiteSpace(v) ? @default : v;
        }

        static int PromptInt(string label, int @default)
        {
            Console.Write(label + ": ");
            var v = Console.ReadLine();
            return int.TryParse(v, out var r) ? r : @default;
        }

        static bool Confirm(string prompt)
        {
            Console.Write(prompt);
            var v = Console.ReadLine();
            return !string.IsNullOrWhiteSpace(v) && v.Trim().Equals("y", StringComparison.OrdinalIgnoreCase);
        }

        static void PrintHelpMain(string worldDir)
        {
            Console.WriteLine("Main menu - choose where to work:");
            Console.WriteLine("  1|world          - World editor (create/list/edit world files)");
            Console.WriteLine("  2|itemdb         - ItemDB editor (manage global items)");
            Console.WriteLine("  3|quests         - Quest editor (create quests inside locations)");
            Console.WriteLine("  help|?           - show this message");
            Console.WriteLine("  exit             - quit the program");
        }

        static void PrintHelpWorld(string worldDir)
        {
            var dbPath = Path.Combine(worldDir, "ItemDB", "items.json");
            var db = LoadItemDb(dbPath);
            Console.WriteLine("World editor commands:");
            Console.WriteLine("  list             - list available world files");
            Console.WriteLine("  new              - create a new location file");
            Console.WriteLine("  edit [file|#]    - edit a location (omit to choose from list)");
            Console.WriteLine($"  itemdb           - switch to ItemDB editor (path: {dbPath}, items: {db.Count})");
            Console.WriteLine("  back             - return to main menu");
            Console.WriteLine("  exit             - quit the program");
        }

        static void PrintHelpEnemies(string worldDir)
        {
            var dbPath = Path.Combine(worldDir, "ItemDB", "items.json");
            var db = LoadItemDb(dbPath);
            Console.WriteLine($"Enemies subcommands (ItemDB: {dbPath}, items: {db.Count}):");
            Console.WriteLine("  add              - add enemies interactively");
            Console.WriteLine("  remove <index>   - remove enemy by index");
            Console.WriteLine("  loot <index>     - manage loot for enemy at index");
            Console.WriteLine("  skip             - skip enemy editing");
        }

        static void PrintHelpLoot(string worldDir, string enemyName)
        {
            var dbPath = Path.Combine(worldDir, "ItemDB", "items.json");
            var db = LoadItemDb(dbPath);
            Console.WriteLine($"Loot subcommands for '{enemyName}' (ItemDB: {dbPath}, items: {db.Count}):");
            Console.WriteLine("  add              - add items from ItemDB to this enemy's loot");
            Console.WriteLine("  remove <index>   - remove a loot item from this enemy");
            Console.WriteLine("  skip             - return to enemy menu");
        }

        static void PrintHelpShop(string worldDir)
        {
            var dbPath = Path.Combine(worldDir, "ItemDB", "items.json");
            var db = LoadItemDb(dbPath);
            Console.WriteLine($"Shop subcommands (ItemDB: {dbPath}, items: {db.Count}):");
            Console.WriteLine("  add              - add items to shop from ItemDB");
            Console.WriteLine("  remove <index>   - remove shop item");
            Console.WriteLine("  skip             - skip shop editing");
        }

        static bool QuestMenu(string worldDir)
        {
            for (; ; )
            {
                Console.WriteLine();
                Console.WriteLine("Quest editor - commands: intro | list | show | create | edit | remove | back | exit | help");
                Console.Write("> ");
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                var cmd = parts[0].ToLowerInvariant();
                if (cmd == "back") return false;
                if (cmd == "exit") return true;
                if (cmd == "help" || cmd == "?")
                {
                    Console.WriteLine("Quest editor commands:");
                    Console.WriteLine("  intro            - make introduction quest for the world");
                    Console.WriteLine("  list             - list locations that contain world files");
                    Console.WriteLine("  show [loc]       - show quests in a location (omit to pick from list)");
                    Console.WriteLine("  create           - create a new quest inside a chosen location");
                    Console.WriteLine("  edit             - edit an existing quest inside a location");
                    Console.WriteLine("  remove           - delete a quest from a location");
                    Console.WriteLine("  back             - return to main menu");
                    Console.WriteLine("  exit             - quit the program");
                    continue;
                }

                if (cmd == "intro")
                {
                    IntroductionQuestMenu(worldDir);
                }

                if (cmd == "list")
                {
                    var files = GetWorldFiles(worldDir);
                    if (files.Count == 0) { Console.WriteLine("No locations found."); continue; }
                    for (int i = 0; i < files.Count; i++) Console.WriteLine($"  [{i}] {files[i].display}");
                    continue;
                }

                // show quests in a location: 'show <locIndex>' or just 'show' to pick
                if (cmd == "show")
                {
                    var files = GetWorldFiles(worldDir);
                    if (files.Count == 0) { Console.WriteLine("No locations found."); continue; }
                    int idx = -1;
                    if (parts.Length > 1 && int.TryParse(parts[1], out var pidx) && pidx >= 0 && pidx < files.Count) idx = pidx;
                    else
                    {
                        Console.WriteLine("Select a location:");
                        for (int i = 0; i < files.Count; i++) Console.WriteLine($"  [{i}] {files[i].display}");
                        Console.Write("> ");
                        var sel = Console.ReadLine();
                        if (!int.TryParse(sel, out idx) || idx < 0 || idx >= files.Count) { Console.WriteLine("Invalid selection"); continue; }
                    }
                    var locationFile = files[idx].path;
                    try
                    {
                        var locText = File.ReadAllText(locationFile);
                        var loc = JsonSerializer.Deserialize<Location>(locText) ?? new Location();
                        if (loc.quests == null || loc.quests.Count == 0) { Console.WriteLine("No quests in this location."); continue; }
                        Console.WriteLine($"Quests in {files[idx].display}:");
                        for (int qi = 0; qi < loc.quests.Count; qi++)
                        {
                            var qq = loc.quests[qi];
                            Console.WriteLine($"  [{qi}] {qq.name} (Lv{qq.level}) xp={qq.xp_reward} credits={qq.credit_reward}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to read location file: " + ex.Message);
                    }
                    continue;
                }

                // edit an existing quest inside a location
                if (cmd == "edit")
                {
                    var files = GetWorldFiles(worldDir);
                    if (files.Count == 0) { Console.WriteLine("No locations available."); continue; }
                    Console.WriteLine("Select a location to edit a quest in:");
                    for (int i = 0; i < files.Count; i++) Console.WriteLine($"  [{i}] {files[i].display}");
                    Console.Write("> ");
                    var sel = Console.ReadLine();
                    if (!int.TryParse(sel, out var locIdx) || locIdx < 0 || locIdx >= files.Count) { Console.WriteLine("Invalid selection"); continue; }
                    var locationFile = files[locIdx].path;
                    try
                    {
                        var locText = File.ReadAllText(locationFile);
                        var loc = JsonSerializer.Deserialize<Location>(locText) ?? new Location();
                        if (loc.quests == null || loc.quests.Count == 0) { Console.WriteLine("No quests in this location."); continue; }
                        Console.WriteLine("Select a quest to edit:");
                        for (int qi = 0; qi < loc.quests.Count; qi++) Console.WriteLine($"  [{qi}] {loc.quests[qi].name}");
                        Console.Write("> ");
                        var qsel = Console.ReadLine();
                        if (!int.TryParse(qsel, out var qidx) || qidx < 0 || qidx >= loc.quests.Count) { Console.WriteLine("Invalid selection"); continue; }
                        var q = loc.quests[qidx];
                        Console.WriteLine("current name: " + q.name);
                        var newName = Prompt("new name (enter to keep)", q.name);
                        if (!string.Equals(newName, q.name, StringComparison.OrdinalIgnoreCase))
                        {
                            // check duplicates
                            if (loc.quests.Exists(x => string.Equals(x.name, newName, StringComparison.OrdinalIgnoreCase)))
                            {
                                Console.WriteLine($"A quest named '{newName}' already exists in this location. Edit aborted.");
                                continue;
                            }
                            q.name = newName;
                        }
                        q.level = PromptInt("level (enter to keep)", q.level);
                        q.description = Prompt("description (enter to keep)", q.description);
                        q.xp_reward = PromptInt("xp_reward (enter to keep)", q.xp_reward);
                        q.credit_reward = PromptInt("credit_reward (enter to keep)", q.credit_reward);
                        q.prerequisite_LVL = PromptInt("prerequisite_LVL (enter to keep)", q.prerequisite_LVL);
                        q.prerequisite_INT = PromptInt("prerequisite_INT (enter to keep)", q.prerequisite_INT);

                        // edit steps
                        Console.WriteLine($"This quest has {q.steps?.Count ?? 0} steps.");
                        if (Confirm("Rebuild steps? (y/N): "))
                        {
                            var newSteps = new List<JsonElement>();
                            while (true)
                            {
                                Console.WriteLine("Add a step type: text | move | items | enemies | done");
                                Console.Write("> ");
                                var st = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();
                                if (st == "done") break;
                                if (st == "text")
                                {
                                    var t = Prompt("text", "");
                                    var obj = new { text = t };
                                    var el = JsonDocument.Parse(JsonSerializer.Serialize(obj, jopts)).RootElement.Clone();
                                    newSteps.Add(el);
                                }
                                else if (st == "move")
                                {
                                    var mv = Prompt("moveTo (location name)", "");
                                    var obj = new { moveTo = mv };
                                    var el = JsonDocument.Parse(JsonSerializer.Serialize(obj, jopts)).RootElement.Clone();
                                    newSteps.Add(el);
                                }
                                else if (st == "items")
                                {
                                    var dbPath = Path.Combine(worldDir, "ItemDB", "items.json");
                                    var db = LoadItemDb(dbPath);
                                    if (db.Count == 0) { Console.WriteLine("ItemDB is empty."); continue; }
                                    var chosenItems = new List<Item>();
                                    while (true)
                                    {
                                        PrintItemDb(db);
                                        Console.Write("pick item index to add (or blank to finish): "); var pick = Console.ReadLine();
                                        if (string.IsNullOrWhiteSpace(pick)) break;
                                        if (!int.TryParse(pick, out var pidx) || pidx < 0 || pidx >= db.Count) { Console.WriteLine("Invalid index"); continue; }
                                        var c = db[pidx];
                                        chosenItems.Add(new Item { name = c.name, icon = c.icon, description = c.description, value = c.value });
                                    }
                                    var obj = new { items = chosenItems };
                                    var el = JsonDocument.Parse(JsonSerializer.Serialize(obj, jopts)).RootElement.Clone();
                                    newSteps.Add(el);
                                }
                                else if (st == "enemies")
                                {
                                    var enemies = new List<Enemy>();
                                    while (true)
                                    {
                                        var en = new Enemy();
                                        en.name = Prompt("enemy name", "Enemy");
                                        en.Level = PromptInt("Level", 1);
                                        en.hp = PromptInt("hp", 1);
                                        en.speed = PromptInt("speed", 1);
                                        en.intellect = PromptInt("int", 1);
                                        en.luck = PromptInt("luck", 1);
                                        en.credits = PromptInt("credits", 0);
                                        enemies.Add(en);
                                        if (!Confirm("Add another enemy? (y/N): ")) break;
                                    }
                                    var obj = new { enemies = enemies };
                                    var el = JsonDocument.Parse(JsonSerializer.Serialize(obj, jopts)).RootElement.Clone();
                                    newSteps.Add(el);
                                }
                                else
                                {
                                    Console.WriteLine("Unknown step type");
                                }
                            }
                            q.steps = newSteps;
                        }

                        // save back
                        loc.quests[qidx] = q;
                        File.WriteAllText(locationFile, JsonSerializer.Serialize(loc, jopts));
                        Console.WriteLine("Quest updated in location file.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to edit quest: " + ex.Message);
                    }
                    continue;
                }

                // remove a quest
                if (cmd == "remove")
                {
                    var files = GetWorldFiles(worldDir);
                    if (files.Count == 0) { Console.WriteLine("No locations available."); continue; }
                    Console.WriteLine("Select a location to remove a quest from:");
                    for (int i = 0; i < files.Count; i++) Console.WriteLine($"  [{i}] {files[i].display}");
                    Console.Write("> ");
                    var sel = Console.ReadLine();
                    if (!int.TryParse(sel, out var locIdx) || locIdx < 0 || locIdx >= files.Count) { Console.WriteLine("Invalid selection"); continue; }
                    var locationFile = files[locIdx].path;
                    try
                    {
                        var locText = File.ReadAllText(locationFile);
                        var loc = JsonSerializer.Deserialize<Location>(locText) ?? new Location();
                        if (loc.quests == null || loc.quests.Count == 0) { Console.WriteLine("No quests in this location."); continue; }
                        Console.WriteLine("Select a quest to remove:");
                        for (int qi = 0; qi < loc.quests.Count; qi++) Console.WriteLine($"  [{qi}] {loc.quests[qi].name}");
                        Console.Write("> ");
                        var qsel = Console.ReadLine();
                        if (!int.TryParse(qsel, out var qidx) || qidx < 0 || qidx >= loc.quests.Count) { Console.WriteLine("Invalid selection"); continue; }
                        if (!Confirm($"Delete quest '{loc.quests[qidx].name}'? (y/N): ")) { Console.WriteLine("Aborted"); continue; }
                        loc.quests.RemoveAt(qidx);
                        File.WriteAllText(locationFile, JsonSerializer.Serialize(loc, jopts));
                        Console.WriteLine("Quest removed.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to remove quest: " + ex.Message);
                    }
                    continue;
                }

                if (cmd == "create")
                {
                    var files = GetWorldFiles(worldDir);
                    if (files.Count == 0) { Console.WriteLine("No locations available to add quests to."); continue; }
                    Console.WriteLine("Select a location to add the quest to:");
                    for (int i = 0; i < files.Count; i++) Console.WriteLine($"  [{i}] {files[i].display}");
                    Console.Write("> ");
                    var sel = Console.ReadLine();
                    if (!int.TryParse(sel, out var idx) || idx < 0 || idx >= files.Count) { Console.WriteLine("Invalid selection"); continue; }
                    var locationFile = files[idx].path;
                    var parent = Path.GetDirectoryName(locationFile) ?? worldDir;
                    string locationFolder;
                    if (string.Equals(Path.GetFullPath(parent).TrimEnd(Path.DirectorySeparatorChar), Path.GetFullPath(worldDir).TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
                        locationFolder = Path.GetFileNameWithoutExtension(locationFile);
                    else
                        locationFolder = Path.GetFileName(parent);

                    // basic quest metadata
                    var q = new Quest();
                    q.name = Prompt("quest name", "New Quest");
                    q.level = PromptInt("level", 1);
                    q.description = Prompt("description", "");
                    q.xp_reward = PromptInt("xp_reward", 0);
                    q.credit_reward = PromptInt("credit_reward", 0);
                    q.prerequisite_LVL = PromptInt("prerequisite_LVL", 1);
                    q.prerequisite_INT = PromptInt("prerequisite_INT", 1);
                    var steps = new List<JsonElement>();

                    // build steps
                    while (true)
                    {
                        Console.WriteLine("Add a step type: text | move | items | enemies | done");
                        Console.Write("> ");
                        var st = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();
                        if (st == "done") break;
                        if (st == "text")
                        {
                            var t = Prompt("text", "");
                            var obj = new { text = t };
                            var el = JsonDocument.Parse(JsonSerializer.Serialize(obj, jopts)).RootElement.Clone();
                            steps.Add(el);
                        }
                        else if (st == "move")
                        {
                            var mv = Prompt("moveTo (location name)", "");
                            var obj = new { moveTo = mv };
                            var el = JsonDocument.Parse(JsonSerializer.Serialize(obj, jopts)).RootElement.Clone();
                            steps.Add(el);
                        }
                        else if (st == "items")
                        {
                            var dbPath = Path.Combine(worldDir, "ItemDB", "items.json");
                            var db = LoadItemDb(dbPath);
                            if (db.Count == 0) { Console.WriteLine("ItemDB is empty."); continue; }
                            var chosenItems = new List<Item>();
                            while (true)
                            {
                                PrintItemDb(db);
                                Console.Write("pick item index to add (or blank to finish): "); var pick = Console.ReadLine();
                                if (string.IsNullOrWhiteSpace(pick)) break;
                                if (!int.TryParse(pick, out var pidx) || pidx < 0 || pidx >= db.Count) { Console.WriteLine("Invalid index"); continue; }
                                var c = db[pidx];
                                chosenItems.Add(new Item { name = c.name, icon = c.icon, description = c.description, value = c.value });
                            }
                            var obj = new { items = chosenItems };
                            var el = JsonDocument.Parse(JsonSerializer.Serialize(obj, jopts)).RootElement.Clone();
                            steps.Add(el);
                        }
                        else if (st == "enemies")
                        {
                            var enemies = new List<Enemy>();
                            while (true)
                            {
                                var en = new Enemy();
                                en.name = Prompt("enemy name", "Enemy");
                                en.Level = PromptInt("Level", 1);
                                en.hp = PromptInt("hp", 1);
                                en.speed = PromptInt("speed", 1);
                                en.intellect = PromptInt("int", 1);
                                en.luck = PromptInt("luck", 1);
                                en.credits = PromptInt("credits", 0);
                                enemies.Add(en);
                                if (!Confirm("Add another enemy? (y/N): ")) break;
                            }
                            var obj = new { enemies = enemies };
                            var el = JsonDocument.Parse(JsonSerializer.Serialize(obj, jopts)).RootElement.Clone();
                            steps.Add(el);
                        }
                        else
                        {
                            Console.WriteLine("Unknown step type");
                        }
                    }

                    q.steps = steps;
                    q.currentStageIndex = 0;

                    // append the new quest to the location's quests array and save the location file
                    try
                    {
                        var locText = File.ReadAllText(locationFile);
                        var locObj = JsonSerializer.Deserialize<Location>(locText) ?? new Location();
                        locObj.quests ??= new List<Quest>();
                        // prevent duplicate quest names (case-insensitive)
                        if (locObj.quests.Exists(x => string.Equals(x.name, q.name, StringComparison.OrdinalIgnoreCase)))
                        {
                            Console.WriteLine($"A quest named '{q.name}' already exists in this location. Creation aborted.");
                        }
                        else
                        {
                            locObj.quests.Add(q);
                            File.WriteAllText(locationFile, JsonSerializer.Serialize(locObj, jopts));
                            Console.WriteLine("Quest added to location file: " + locationFile);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to update location file with new quest: " + ex.Message);
                    }
                    continue;
                }
            }
        }

        static bool IntroductionQuestMenu(string worldDir)
        {
            for (; ; )
            {
                Console.WriteLine();
                Console.WriteLine("Introduction Quest editor - commands: create | edit | back | help | ?");
                Console.Write("> ");
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                var cmd = parts[0].ToLowerInvariant();
                if (cmd == "back") return false;
                // if (cmd == "exit") return true;
                if (cmd == "help" || cmd == "?")
                {
                    Console.WriteLine("Introduction Quest editor commands:");
                    Console.WriteLine("  create           - create an introduction quest (only one allowed, will overwrite existing intro.json)");
                    Console.WriteLine("  edit             - edit the existing introduction quest (if intro.json exists)");
                    Console.WriteLine("  back             - return to main menu");
                    continue;
                }

                if (cmd == "create")
                {
                    var introPath = Path.Combine(worldDir, "intro.json");
                    if (File.Exists(introPath) && !Confirm("intro.json already exists. Overwrite? (y/N): "))
                    {
                        Console.WriteLine("Creation aborted.");
                        continue;
                    }
                    var introQuest = new Quest
                    {
                        name = "Introduction",
                        description = "This is the introduction quest for the world.",
                        level = 1,
                        xp_reward = 0,
                        credit_reward = 0,
                        prerequisite_LVL = 1,
                        prerequisite_INT = 1,
                        steps = new List<JsonElement>
                    {
                        JsonDocument.Parse(JsonSerializer.Serialize(new { text = "Welcome to the world! This is your introduction quest." }, jopts)).RootElement.Clone()
                    }
                    };

                    File.WriteAllText(introPath, JsonSerializer.Serialize(introQuest, jopts));
                    Console.WriteLine("Introduction quest created at: " + introPath);
                    Console.WriteLine("Do you want to edit the introduction quest?" + " (y/N): ");
                    var editIntro = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(editIntro) && editIntro.Trim().Equals("y", StringComparison.OrdinalIgnoreCase))
                    {
                        EditIntroQuest(introPath, worldDir);
                    }

                }
                else if (cmd == "edit")
                {
                    var introPath = Path.Combine(worldDir, "intro.json");
                    if (!File.Exists(introPath)) { Console.WriteLine("No intro.json found. Use 'create' command to make one."); continue; }
                    try
                    {
                        EditIntroQuest(introPath, worldDir);
                        Console.WriteLine("Introduction quest updated at: " + introPath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to edit introduction quest: " + ex.Message);
                    }
                }
            }
        }
        // ImportFromCsServer2 removed

        static void EditIntroQuest(string introPath, string worldDir)
        {
            var introText = File.ReadAllText(introPath);
            var introQuest = JsonSerializer.Deserialize<Quest>(introText) ?? new Quest();
            Console.WriteLine("Editing introduction quest: " + introQuest.name);
            // basic quest metadata
            var q = new Quest();
            q.name = Prompt("quest name", "New Quest");
            q.level = PromptInt("level", 1);
            q.description = Prompt("description", "");
            q.xp_reward = PromptInt("xp_reward", 0);
            q.credit_reward = PromptInt("credit_reward", 0);
            q.prerequisite_LVL = PromptInt("prerequisite_LVL", 1);
            q.prerequisite_INT = PromptInt("prerequisite_INT", 1);
            var steps = new List<JsonElement>();

            // build steps
            while (true)
            {
                Console.WriteLine("Add a step type: text | move | items | enemies | done");
                Console.Write("> ");
                var st = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();
                if (st == "done") break;
                if (st == "text")
                {
                    var t = Prompt("text", "");
                    var obj = new { text = t };
                    var el = JsonDocument.Parse(JsonSerializer.Serialize(obj, jopts)).RootElement.Clone();
                    steps.Add(el);
                }
                else if (st == "move")
                {
                    var mv = Prompt("moveTo (location name)", "");
                    var obj = new { moveTo = mv };
                    var el = JsonDocument.Parse(JsonSerializer.Serialize(obj, jopts)).RootElement.Clone();
                    steps.Add(el);
                }
                else if (st == "items")
                {
                    var dbPath = Path.Combine(worldDir, "ItemDB", "items.json");
                    var db = LoadItemDb(dbPath);
                    if (db.Count == 0) { Console.WriteLine("ItemDB is empty."); continue; }
                    var chosenItems = new List<Item>();
                    while (true)
                    {
                        PrintItemDb(db);
                        Console.Write("pick item index to add (or blank to finish): "); var pick = Console.ReadLine();
                        if (string.IsNullOrWhiteSpace(pick)) break;
                        if (!int.TryParse(pick, out var pidx) || pidx < 0 || pidx >= db.Count) { Console.WriteLine("Invalid index"); continue; }
                        var c = db[pidx];
                        chosenItems.Add(new Item { name = c.name, icon = c.icon, description = c.description, value = c.value });
                    }
                    var obj = new { items = chosenItems };
                    var el = JsonDocument.Parse(JsonSerializer.Serialize(obj, jopts)).RootElement.Clone();
                    steps.Add(el);
                }
                else if (st == "enemies")
                {
                    var enemies = new List<Enemy>();
                    while (true)
                    {
                        var en = new Enemy();
                        en.name = Prompt("enemy name", "Enemy");
                        en.Level = PromptInt("Level", 1);
                        en.hp = PromptInt("hp", 1);
                        en.speed = PromptInt("speed", 1);
                        en.intellect = PromptInt("int", 1);
                        en.luck = PromptInt("luck", 1);
                        en.credits = PromptInt("credits", 0);
                        enemies.Add(en);
                        if (!Confirm("Add another enemy? (y/N): ")) break;
                    }
                    var obj = new { enemies = enemies };
                    var el = JsonDocument.Parse(JsonSerializer.Serialize(obj, jopts)).RootElement.Clone();
                    steps.Add(el);
                }
                else
                {
                    Console.WriteLine("Unknown step type");
                }
            }

            q.steps = steps;
            q.currentStageIndex = 0;

            File.WriteAllText(introPath, JsonSerializer.Serialize(q, jopts));
        }

        static string? ResolveWorldPath(string worldDir, string input)
        {
            // normalize separators
            var normalized = input.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);

            // 1) direct path under worldDir
            var candidate = Path.Combine(worldDir, normalized);
            if (File.Exists(candidate)) return candidate;

            // 2) add .json if missing
            if (!Path.HasExtension(candidate))
            {
                var cand2 = candidate + ".json";
                if (File.Exists(cand2)) return cand2;
            }

            // 3) look inside each subdirectory for a matching file
            foreach (var d in Directory.GetDirectories(worldDir))
            {
                var nameOnly = Path.GetFileName(normalized);
                var try1 = Path.Combine(d, nameOnly);
                if (File.Exists(try1)) return try1;
                if (!Path.HasExtension(try1))
                {
                    var try2 = try1 + ".json";
                    if (File.Exists(try2)) return try2;
                }

                // also if input was a folder name, check for <folder>/<folder>.json
                var folderName = Path.GetFileName(d);
                if (string.Equals(folderName, normalized, StringComparison.OrdinalIgnoreCase))
                {
                    var defaultFile = Path.Combine(d, folderName + ".json");
                    if (File.Exists(defaultFile)) return defaultFile;
                }
            }
            return null;
        }
    }
}