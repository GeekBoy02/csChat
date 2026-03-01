using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text.Json;
using SocketServer;

/// <summary>
/// A simple console application to manage user JSON files.
/// Allows you to create, list, edit, and delete users.
/// </summary>
class Program
{
    static JsonSerializerOptions jopts = new JsonSerializerOptions { WriteIndented = true };

    static void Main(string[] args)
    {
        var exeDir = AppContext.BaseDirectory;
        var projectRoot = Path.GetFullPath(Path.Combine(exeDir, "..", "..", "..", ".."));
        var userDir = Path.Combine(projectRoot, "csServer2", "users");
        Directory.CreateDirectory(userDir);
        Console.WriteLine("Server Manager - simple console tool to create/list/edit user JSON files");

        // Main menu loop
        for (; ; )
        {
            Console.WriteLine();
            Console.WriteLine("Main menu" + Environment.NewLine + "  list | create | edit | delete | help | ? | exit");
            Console.Write("> ");
            var line = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(line)) continue;
            line = line.Trim();
            if (line.Equals("list", StringComparison.OrdinalIgnoreCase))
            {
                var files = GetUserFiles(userDir, true);
                if (files.Count == 0) Console.WriteLine("No user files found.");
                for (int i = 0; i < files.Count; i++) Console.WriteLine(files[i].display);
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                continue;
            }
            if (line.Equals("create", StringComparison.OrdinalIgnoreCase))
            {
                SocketServer.User newUser = new SocketServer.User("placeholder", "placeholder");
                newUser.Name = Prompt("Enter username: ", "defaultuser");

                var filename = Path.Combine(userDir, newUser.Name + ".json");
                var json = JsonSerializer.Serialize(newUser, jopts);
                File.WriteAllText(filename, json);
                Console.WriteLine("Created: " + Path.GetFullPath(filename));
                continue;
            }
            if (line.Equals("edit", StringComparison.OrdinalIgnoreCase))
            {
                var filename = Prompt("Enter username to edit (without .json)", "");
                if (!filename.EndsWith(".json")) filename += ".json";
                var filePath = Path.Combine(userDir, filename);
                if (!File.Exists(filePath))
                {
                    Console.WriteLine("File does not exist: " + filePath);
                    continue;
                }
                var success = EditUserMenu(userDir, filePath);
                if (!success) continue;
                // var exitRequested = DeleteUserMenu(userDir);
                // if (exitRequested) break;
                continue;
            }
            if (line.Equals("delete", StringComparison.OrdinalIgnoreCase))
            {
                var files = GetUserFiles(userDir, true);
                if (files.Count == 0)
                {
                    Console.WriteLine("No user files found.");
                    continue;
                }
                Console.WriteLine("Select user to delete:");
                for (int i = 0; i < files.Count; i++) Console.WriteLine($"  {i + 1}. {files[i].display}");
                var input = Prompt("Enter number or username", "");

                string filePath = null;
                if (int.TryParse(input, out var choice))
                {
                    if (choice < 1 || choice > files.Count)
                    {
                        Console.WriteLine("Invalid selection.");
                        continue;
                    }
                    filePath = files[choice - 1].path;
                }
                else
                {
                    filePath = Path.Combine(userDir, input + ".json");
                    if (!File.Exists(filePath))
                    {
                        Console.WriteLine("User not found.");
                        continue;
                    }
                }

                File.Delete(filePath);
                Console.WriteLine("Deleted: " + Path.GetFullPath(filePath));
                continue;
            }
            if (line.Equals("?") || line.Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Available commands:");
                Console.WriteLine("  list     - List all user files");
                Console.WriteLine("  create   - Create a new user file");
                Console.WriteLine("  edit     - Edit an existing user file, when editing leave values empty to keep current value");
                Console.WriteLine("  delete   - Delete an existing user file");
                Console.WriteLine("  help/?   - Show this help message");
                Console.WriteLine("  exit     - Exit the application");
                continue;
            }
            if (line.Equals("exit", StringComparison.OrdinalIgnoreCase) || line.Equals("4")) break;
            Console.WriteLine("Unknown selection, type 'help' or '?' to see options.");
        }
    }
    static List<(string display, string path)> GetUserFiles(string userDir)
    {
        var result = new List<(string, string)>();
        foreach (var f in Directory.GetFiles(userDir, "*.json")) result.Add((Path.GetFileName(f), f));
        return result;
    }

    static List<(string display, string path)> GetUserFiles(string userDir, bool excludeFileNameExtension)
    {
        var result = new List<(string, string)>();
        if (excludeFileNameExtension)
        {
            foreach (var f in Directory.GetFiles(userDir)) result.Add((Path.GetFileNameWithoutExtension(f), f));
        }
        else
        {
            foreach (var f in Directory.GetFiles(userDir, "*.json")) result.Add((Path.GetFileName(f), f));
        }
        return result;
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
    static bool EditUserMenu(string userDir, string filePath)
    {
        var oldFile = Path.GetFileName(filePath);
        var json = File.ReadAllText(filePath);
        var user = JsonSerializer.Deserialize<SocketServer.User>(json);
        if (user == null)
        {
            Console.WriteLine("Failed to parse user file.");
            return false;
        }
        Console.WriteLine("Editing user: " + user.Name);
        Console.WriteLine("Press Enter to keep current value.");

        var newName = Prompt("Username", user.Name);
        if (!newName.Equals(user.Name, StringComparison.OrdinalIgnoreCase))
        {
            var newFilePath = Path.Combine(userDir, newName + ".json");
            if (File.Exists(newFilePath))
            {
                Console.WriteLine("A user with that name already exists. Edit cancelled.");
                return false;
            }
            user.Name = newName;
            filePath = newFilePath;
        }

        user.IsDead = Prompt("Is the user dead (true/false)", user.IsDead.ToString()).Equals("true", StringComparison.OrdinalIgnoreCase);

        // var classes = new[] { "Soldier", "Engineer", "Explorer" };
        var classes = new[] { "Soldier", "Engineer", "Explorer" };
        Console.WriteLine("Select class:");
        for (int i = 0; i < classes.Length; i++) Console.WriteLine($"  {i + 1}. {classes[i]}");
        var classChoice = PromptInt("Class (1-3)", 1);

        switch (classChoice)
        {
            case 1:
                user.ChangeTo_Soldier();
                break;
            case 2:
                user.ChangeTo_Engineer();
                break;
            case 3:
                user.ChangeTo_Explorer();
                break;
            default:
                user.ChangeTo_Soldier();
                Console.WriteLine("Invalid selection, defaulting to Soldier.");
                break;
        }
        user.Level = PromptInt("Level", user.Level);
        user.Xp = PromptInt("Xp", user.Xp);
        user.Credits = PromptInt("Credits", user.Credits);
        user.Speed = PromptInt("Speed", user.Speed);
        user.Intellect = PromptInt("Intellect", user.Intellect);
        user.Luck = PromptInt("Luck", user.Luck);
        user.Hp = PromptInt("Hp", user.Hp);

        Console.WriteLine("Delete the old user file? (y/N)");
        var deleteOld = Console.ReadLine();
        if (deleteOld.Equals("y", StringComparison.OrdinalIgnoreCase))
        {
            File.Delete(Path.Combine(userDir, oldFile));
            Console.WriteLine("old userfile deleted: " + Path.Combine(userDir, oldFile));
        }

        var newJson = JsonSerializer.Serialize(user, jopts);
        File.WriteAllText(filePath, newJson);
        Console.WriteLine("User updated: " + Path.GetFullPath(filePath));

        return true;
    }
}