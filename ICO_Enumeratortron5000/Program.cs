using System;
using System.Collections.Generic;
using System.IO;
using System.Web;

namespace ICO_Enumeratortron5000
{
    class Program
    {
        static string icoFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\ICO_Enumerator5000\\";
        static string backupFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\ICO_Enumerator5000\\Backups\\";
        static string workingDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        static bool ignoreExisting = false;
        static int finishArgument = 0;
        static bool autoApprove = false;
        static void Main(string[] args)
        {
            // -d Directory Of shortcuts
            // -i Ignore Existing Icons (Redownload them all)
            // -f Finish Argument 0-Nothing 1-Refresh Explorer 2-ieuinit
            // -h Shows help
            int i = 0;
            
            foreach(string arg in args)
            {
                switch (arg)
                {
                    default:
                        break;
                    case "-d":
                        workingDir = args[i + 1];

                        if (!Directory.Exists(workingDir))
                        {
                            Console.WriteLine(workingDir);
                            Console.WriteLine("Target directory doesn't exist. Create it? Y/N");
                            if (Console.ReadLine().ToUpper() == "Y")
                                Directory.CreateDirectory(workingDir);
                            else
                                Environment.Exit(0);
                        }
                            
                        break;
                    case "-i":
                        ignoreExisting = true;
                        break;
                    case "-f":
                        Int32.TryParse(args[i + 1], out finishArgument);
                        break;
                    case "-y":
                        autoApprove = true;
                        break;
                    case "-h":
                        Console.WriteLine(
                            "ICO Enumerator 5000 v1.0.0\n" +
                            "Developed by ManMadeOfGouda\n" +
                            "https://joexv.github.io/ \n" +
                            "https://github.com/joexv/ \n\n" +
                            "-d [PathToURLs]\n" +
                            "Optional Directory where you keep your URL shortcuts default is the Desktop.\n\n" +
                            "-i [No arguments]\n" +
                            "When included, all icons will be redownloaded regardless of if they exist or not.\n\n" +
                            "-f [0,1,2]\n" +
                            "0 - Program will do nothing when done.\n" +
                            "1 - IconCache.db will be deleted and File Explorer restarted\n" +
                            "2 - Runs the command ie4uinit.exe -show (Windows 8.1 and below)\n\n");
                        break;
                }
                i++;
            }
            //Main Work
            Directory.CreateDirectory(icoFolder);
            Directory.CreateDirectory(backupFolder);
            Dictionary<FileInfo, String> files = GetDesktopLinks();

            Console.WriteLine("Do the URLs look correct?");
            if (autoApprove || Console.ReadLine().ToUpper() == "Y")
            {
                foreach (KeyValuePair<FileInfo, String> file in files)
                {
                    if (DownloadFavicon(file.Value))
                    {
                        string url = new Uri(file.Value).GetLeftPart(UriPartial.Authority);
                        string parsed = url.Replace(@"http://", "").Replace("https://", "");
                        parsed = parsed.Replace("/", "");
                        parsed = ReplaceInvalidChars(parsed);

                        string ico = $"{icoFolder}{parsed}.ico";

                        ChangeICO(file.Key, ico);
                    }
                }
            }

            switch (finishArgument)
            {
                case 1:
                    RefreshIconDB();
                    break;
                case 2:
                    cmd("ie4uinit.exe -show");
                    break;
                default:
                    DirectoryInfo d = new DirectoryInfo(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
                    foreach (FileInfo file in d.GetFiles("*.png"))
                        File.Delete(file.FullName);
                    Console.WriteLine("Done, press Enter/Return to close...");
                    Console.ReadLine();
                    break;

            }
        }

        static private Dictionary<FileInfo, String> GetDesktopLinks()
        {
            Dictionary<FileInfo, String> Results = new Dictionary<FileInfo, String> { };
            DirectoryInfo d = new DirectoryInfo(workingDir);
            Console.WriteLine(workingDir);

            foreach (FileInfo file in d.GetFiles("*.url"))
            {
                string lnkContents = File.ReadAllText(file.FullName);
                string[] split = lnkContents.Split('\n');
                int urlIndex = Array.FindIndex(split, str => str.Contains("URL"));

                if (urlIndex != -1)
                {
                    var url = split[urlIndex].Substring(4);
                    if (url.Substring(0, 4).ToUpper().Contains("HTTP"))
                    {
                        Results.Add(file, url);
                        if(!File.Exists(backupFolder + file.Name))
                            File.Copy(file.FullName, backupFolder + file.Name);
                        Console.WriteLine($"{url}\n");
                    }
                }
            }
            return Results;
        }

        static private bool DownloadFavicon(string URL)
        {
            string url = new Uri(URL).GetLeftPart(UriPartial.Authority);
            string parsed = url.Replace(@"http://", "").Replace("https://", "");
            parsed = parsed.Replace("/", "");
            parsed = ReplaceInvalidChars(parsed);

            var client = new System.Net.WebClient();
            //TODO Add more/alternative icon sources
            Uri uri = new Uri("https://icon.horse/icon");
            uri = uri.AddParameter("uri", HttpUtility.UrlEncode(new Uri(URL).GetLeftPart(UriPartial.Authority)));

            string PNG = parsed + ".png";

            Console.WriteLine($"Downloading {PNG}...");
            if (ignoreExisting || !File.Exists($"{icoFolder}{parsed}.ico"))
            {
                client.DownloadFile(uri, PNG);
                if (File.Exists(PNG))
                {
                    Console.WriteLine("Converting...");
                    return ImagingHelper.ConvertToIcon(PNG, $"{icoFolder}{parsed}.ico", 512, true);
                }
                else
                {
                    Console.WriteLine("Failed to download " + PNG);
                    return false;
                }
            }
            else
            {
                Console.WriteLine($"ICO already exists for {parsed}, skipping download...");
                return true;
            }
            
            
        }

        static private void ChangeICO(FileInfo file, string ICO)
        {
            string lnkContents = File.ReadAllText(file.FullName);
            Console.WriteLine(lnkContents + "\n");
            string[] split = lnkContents.Split('\n');

            int icoIndex = Array.FindIndex(split, str => str.Contains("IconFile")); ;
            if (icoIndex == -1)
            {
                Console.WriteLine("Shortcut doesn't have Icon, recreating file contents...");
                File.WriteAllText(file.FullName, $"{lnkContents}IconIndex=0\nHotKey=0\nIconFile={ICO}");
            }
            else
            {
                Console.WriteLine("Shortcut has existing icon, replacing...");
                split[icoIndex] = $"IconFile={ICO}";
                File.WriteAllLines(file.FullName, split);
            }
        }

        static public string ReplaceInvalidChars(string filename)
        {
            return string.Join("", filename.Split(Path.GetInvalidFileNameChars()));
        }

        static private void RefreshIconDB()
        {
            cmd(@"taskkill /IM explorer.exe /F");
            cmd(@"CD /d %userprofile%\AppData\Local");
            cmd(@"DEL IconCache.db /a");

            System.Diagnostics.Process.Start("explorer.exe");
        }

        static private void cmd(string Command)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = $"/C {Command}";
            process.StartInfo = startInfo;
            process.Start();
        }
    }

    public static class UriExtensions
    {
        /// <summary>
        /// Adds the specified parameter to the Query String.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="paramName">Name of the parameter to add.</param>
        /// <param name="paramValue">Value for the parameter to add.</param>
        /// <returns>Url with added parameter.</returns>
        public static Uri AddParameter(this Uri url, string paramName, string paramValue)
        {
            var uriBuilder = new UriBuilder(url);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query[paramName] = paramValue;
            uriBuilder.Query = query.ToString();

            return uriBuilder.Uri;
        }
    }
}
