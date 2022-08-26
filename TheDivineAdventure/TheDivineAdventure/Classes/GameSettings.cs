using System.IO;

namespace TheDivineAdventure
{
    class GameSettings
    {

        public GameSettings()
        {
        }

        private static void WriteFile(string[] text, string fileName)
        {
            string filePath = (Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.ToString()+@"\"+fileName);

            File.WriteAllLines(filePath, text);
        }

        //reads a file in the current directory
        public static string[,] ReadSettings()
        {
            string filePath = (Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.ToString() + @"\Settings.txt");
            string[] text = File.ReadAllLines(filePath);
            string[,] output = new string[text.Length, 2];
            int i = 0;
            foreach (string line in text)
            {
                output[i, 0] = line.Substring(0, line.LastIndexOf(':'));
                output[i, 1] = line.Substring(line.LastIndexOf('-')+2);
                i++;
            }

            return output;
        }

        //writes the settings
        public static void WriteSettings(string[,] settings)
        {
            string[] output = new string[7];

            output[0] = "Screen Width: - " + settings[0,1];
            output[1] = "Screen Height: - " + settings[1,1];
            output[2] = "Window Mode: - " + settings[2,1];
            output[3] = "Antialiasing: - " + settings[3,1];
            output[4] = "Master Volume: - " + settings[4,1];
            output[5] = "Music Volume: - " + settings[5,1];
            output[6] = "SFX Volume: - " + settings[6,1];

            WriteFile(output, "Settings.txt");
        }

        public static bool HasSettings()
        {
            if (File.Exists(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.ToString() + @"\Settings.txt"))
                return true;
            return false;
        }
    }
}
