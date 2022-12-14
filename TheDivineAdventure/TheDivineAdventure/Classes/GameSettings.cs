using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

namespace TheDivineAdventure
{
    class GameSettings
    {
        public static readonly string[] defaultSettings = { "1920", "1080", "1", "0", "0.5", "0.5", "0.5", "38" };
        public static IDictionary<string, float> Settings = new Dictionary<string, float>();

        public GameSettings()
        {
        }

        public static void WriteFile(string[] text, string fileName)
        {
            string filePath = (Directory.GetCurrentDirectory()+@"\"+fileName);
            File.WriteAllLines(filePath, text);
        }

        //reads a file in the current directory
        public static string[,] ReadSettings()
        {

            string filePath = (Directory.GetCurrentDirectory() + @"\Settings.txt");
            string[] text = File.ReadAllLines(filePath);
            string[,] output = new string[text.Length, 2];
            int i = 0;

            foreach (string line in text)
            {
                if (line.StartsWith("--")) continue;
                if (line == "") break;
                output[i, 0] = line.Substring(0, line.LastIndexOf(':'));
                output[i, 1] = line.Substring(line.LastIndexOf('-')+2);

                Settings[line.Substring(0, line.LastIndexOf(':')).Replace(" ", string.Empty)]  = float.Parse(line.Substring(line.LastIndexOf('-') + 2));

                i++;
            }

            //Update volumes to be used
            Settings["MusicVolume"] *= Settings["MasterVolume"];
            Settings["SFXVolume"] *= Settings["MasterVolume"];
            return output;
        }

        //writes the settings
        public static void WriteSettings(string[,] settings)
        {
            string[] output = new string[10];

            output[0] = "Screen Width: - " + settings[0,1];
            output[1] = "Screen Height: - " + settings[1,1];
            output[2] = "Window Mode: - " + settings[2,1];
            output[3] = "Antialiasing: - " + settings[3,1];
            output[4] = "Master Volume: - " + settings[4,1];
            output[5] = "Music Volume: - " + settings[5,1];
            output[6] = "SFX Volume: - " + settings[6,1];
            output[7] = "--Controls--";
            output[8] = "Mouse Sensitivty: - " + settings[7, 1];

            for (int i = 0; i < settings.Length-1; i++)
            {
                if (settings[i, 0] == null || settings[i, 1] == null) break;
                Settings[settings[i, 0]] = float.Parse(settings[i, 1]);
            }
            //Update volumes to be used
            Settings["MusicVolume"] *= Settings["MasterVolume"];
            Settings["SFXVolume"] *= Settings["MasterVolume"];

            WriteFile(output, "Settings.txt");
        }

        //writes the settings from default
        public static void WriteSettings()
        {
            string[] output = new string[10];

            output[0] = "Screen Width: - " + defaultSettings[0];
            Settings["ScreenWidth"] = float.Parse(defaultSettings[0]);

            output[1] = "Screen Height: - " + defaultSettings[1];
            Settings["ScreenHeight"] = float.Parse(defaultSettings[1]);

            output[2] = "Window Mode: - " + defaultSettings[2];
            Settings["WindowMode"] = float.Parse(defaultSettings[2]);

            output[3] = "Antialiasing: - " + defaultSettings[3];
            Settings["Antialiasing"] = float.Parse(defaultSettings[3]);

            output[4] = "Master Volume: - " + defaultSettings[4];
            Settings["MasterVolume"] = float.Parse(defaultSettings[4]);

            output[5] = "Music Volume: - " + defaultSettings[5];
            Settings["MusicVolume"] = float.Parse(defaultSettings[5]) * float.Parse(defaultSettings[4]);

            output[6] = "SFX Volume: - " + defaultSettings[6];
            Settings["SFXVolume"] = float.Parse(defaultSettings[6]) * float.Parse(defaultSettings[4]);

            output[7] = "--Controls--";
            output[8] = "Mouse Sensitivty: - " + defaultSettings[7];
            Settings["MouseSensitivty"] = float.Parse(defaultSettings[7]);

            WriteFile(output, "Settings.txt");
        }

        public static bool HasSettings()
        {
            if (File.Exists(Directory.GetCurrentDirectory() + @"\Settings.txt"))
                return true;
            WriteSettings();
            return false;
        }
    }
}
