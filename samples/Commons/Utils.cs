using System;
using System.IO;
using System.Text.Json;

namespace KodyCommons;

public class Utils
{
    // Method to find a file in the current directory or its parent directories
    public static string FindFileInDirectoryOrParents(string directory, string fileName)
    {
        // Check if file exists in the current directory
        var filePath = Path.Combine(directory, fileName);
        if (File.Exists(filePath))
        {
            return filePath;
        }

        // If we reached the root directory, stop searching
        var parentDirectory = Directory.GetParent(directory);
        if (parentDirectory == null) {
            return null;
        }

        // Recursively search the parent directory
        return FindFileInDirectoryOrParents(parentDirectory.FullName, fileName);
    }

    // Method to load and deserialize settings
    public static KodySettings LoadSettings(string fileName = "appsettings.json")
    {
        // Get the current working directory
        var workingDirectory = Directory.GetCurrentDirectory();

        // Use the file search method to find the file
        var filePath = FindFileInDirectoryOrParents(workingDirectory, fileName);

        if (filePath != null) {
            // Read the file and deserialize the JSON into the KodySettings object
            string jsonContent = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<KodySettings>(jsonContent);
        }
        else
        {
            throw new FileNotFoundException($"Settings file '{fileName}' not found in the current directory or any parent directories.");
        }
    }
    
    // Function to perform a countdown
    public static void StartCountdown(String message, int seconds)
    {
        Console.WriteLine(message, seconds);
        PrintWithPadding($"Time remaining: {seconds} seconds");

        while (seconds > 0)
        {
            // Move the cursor to the position of the last line
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            PrintWithPadding($"Time remaining: {seconds} seconds");
            seconds--;

            // Sleep for 1 second (1000 milliseconds)
            Thread.Sleep(1000);
        }

        // Once countdown is finished
        Console.SetCursorPosition(0, Console.CursorTop - 1);
        PrintWithPadding("Time is up...");
    }

    // Function to print text with padding to clear old text
    public static void PrintWithPadding(string text)
    {
        int consoleWidth = Console.WindowWidth;
        // Ensure the line is padded with spaces to clear any previous content
        string paddedText = text.PadRight(consoleWidth);
        Console.WriteLine(paddedText);
    }
}

public record KodySettings(string Address, string StoreId, string ApiKey);
