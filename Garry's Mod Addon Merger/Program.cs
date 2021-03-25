using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Newtonsoft.Json;

namespace Tamewater.GMOD_AddonMerger
{
    class Program
    {
        // Constants.
        private static readonly string STARTUP_PATH = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
        private static readonly string DEFAULT_JSON_PATH = $"{STARTUP_PATH}\\ignore.json";
        private static readonly string DEFAULT_MERGE_DIR = "_MERGED";

        // Globals.
        private static int MAX_THREADS = (int)Math.Ceiling((double)Environment.ProcessorCount / 2);
        private static int ADDONS_PER_THREAD = 20;
        private static bool OVERWRITE_CONFLICTS = false;
        private static bool LOG = false;

        private static int Main(string[] args)
        {
            // Parse arguments.
            Dictionary<string, string> arguments = ParseArguments(args);

            // Locals.
            string addonDirectory;
            string outputDirectory;
            List<string> excludedAddons;

            // No arguments, run in regular window + console mode.
            if (arguments == null)
            {
                // Load excluded addons from default path.
                excludedAddons = LoadExcludedAddons(DEFAULT_JSON_PATH);

                // Get the addons directory.
                do
                {
                    Console.WriteLine("Enter a folder that contains your addons...");
                    addonDirectory = Console.ReadLine();
                } while (!Path.IsPathRooted(addonDirectory));

                // Get all addon directories.
                string[] addons = Directory.GetDirectories(addonDirectory);
                Console.WriteLine("Found {0} folders within {1}", addons.Length, addonDirectory);
                // Filter addons.
                List<string> finalAddons = FilterAddons(excludedAddons, addons);

                // Check if we have any addons left.
                if (finalAddons.Count == 0)
                {
                    Console.WriteLine("No addons to merge. Please restart and try again.");
                    Pause();
                }
                else
                {
                    // Get an output directory.
                    do
                    {
                        Console.WriteLine("Enter an output folder, entering \"default\" will default to {0}\\{1}", addonDirectory, DEFAULT_MERGE_DIR);
                        outputDirectory = Console.ReadLine();
                        Console.WriteLine(outputDirectory.ToLower() != "default" || !Path.IsPathRooted(outputDirectory));
                    } while (outputDirectory.ToLower() != "default" && !Path.IsPathRooted(outputDirectory));

                    // Resort to default.
                    if (outputDirectory.ToLower() == "default")
                        outputDirectory = $"{addonDirectory}\\{DEFAULT_MERGE_DIR}";

                    // Start the merging process.
                    MergeAddons(finalAddons, outputDirectory);
                }
            }
            // Got arguments, run in command line.
            else
            {
                // Check if the help argument was passed.
                if (arguments.ContainsKey("help"))
                {
                    Console.WriteLine("\nGarry's Mod Addon Merger");
                    Console.WriteLine("Valid arguments to provide to this executable are:");
                    Console.WriteLine("\t-help : Provides this help menu.");
                    Console.WriteLine("\t-addons : The path to the addons folder to merge.");
                    Console.WriteLine("\t-output : The path to the output folder. (Omit to place it in -addons argument under the \"{0}\" directory)", DEFAULT_MERGE_DIR);
                    Console.WriteLine("\t-maxThreads : The max amount of processing threads that can be created.");
                    Console.WriteLine("\t-addonsPerThread : The max amount of addons that can be provided to each thread. (Ignored if the max threads is exceeded)");
                    Console.WriteLine("\t-ignoreJSON : Path to a JSON file that is an array of addon names that can be ignored.");
                    Console.WriteLine("\t-overwriteConflicts : Flag that will allow the program to overwrite existing files.");
                    Console.WriteLine("\t-log : Flag that will let errors and conflicts be logged.");
                    Console.WriteLine("Execution Examples:");
                    Console.WriteLine("\t\"Garry's Mod Addon Merger.exe\" -help");
                    Console.WriteLine("\t\"Garry's Mod Addon Merger.exe\" -addons C:\\GMOD_Servers\\Server\\garrysmod\\addons\n");

                    Pause();
                }
                else
                {
                    // Check if we got an addons directory to merge.
                    if (!arguments.ContainsKey("addons"))
                    {
                        Console.WriteLine("No addons folder path provided! Use -help if you're unsure of how to use this program.");
                        Pause();
                        return -1;
                    }
                    addonDirectory = arguments["addons"];

                    // Check if the directory path we got is valid.
                    try
                    {
                        Path.GetFullPath(addonDirectory);
                        if (!Path.IsPathRooted(addonDirectory))
                            throw new Exception();
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Received invalid addons folder! Use -help if you're unsure of how to use this program.");
                        Pause();
                        return -1;
                    }

                    // Check if we got an output directory. Resort to default if not.
                    if (!arguments.ContainsKey("output"))
                        outputDirectory = $"{addonDirectory}\\{DEFAULT_MERGE_DIR}";
                    else
                        outputDirectory = arguments["output"];

                    // Check if the output directory path we have is valid.
                    try
                    {
                        Path.GetFullPath(outputDirectory);
                        if (!Path.IsPathRooted(outputDirectory))
                            throw new Exception();
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Received invalid output folder! Use -help if you're unsure of how to use this program.");
                        Pause();
                        return -1;
                    }

                    // Set max threads if it is passed.
                    if (arguments.ContainsKey("maxThreads"))
                        int.TryParse(arguments["maxThreads"], out MAX_THREADS);

                    // Set addons per thread if it is passed.
                    if (arguments.ContainsKey("addonsPerThread"))
                        int.TryParse(arguments["addonsPerThread"], out ADDONS_PER_THREAD);

                    // Load custom ignore JSON if it is passed. Load default otherwise.
                    if (arguments.ContainsKey("ignoreJSON"))
                        excludedAddons = LoadExcludedAddons(arguments["ignoreJSON"]);
                    else
                        excludedAddons = LoadExcludedAddons(DEFAULT_JSON_PATH);

                    // Set overwrite conflicts if the flag is there.
                    if (arguments.ContainsKey("overwriteConflicts"))
                        OVERWRITE_CONFLICTS = true;

                    // Set log if the flag is there.
                    if (arguments.ContainsKey("log"))
                        LOG = true;

                    // Get all addon folders.
                    Console.WriteLine("Arguments good, getting addons at {0}", addonDirectory);
                    string[] addons = Directory.GetDirectories(addonDirectory);
                    // Filter and start merging process.
                    Console.WriteLine("Filtering addons and starting merge...");
                    MergeAddons(FilterAddons(excludedAddons, addons), outputDirectory);
                }
            }

            // Return okay status.
            return 0;
        }

        /// <summary>
        /// Parses arguments given in command-line.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <returns>A neatly formatted dictionary of the arguments and their values.</returns>
        private static Dictionary<string, string> ParseArguments(string[] args)
        {
            // Bail if args is empty.
            if (args.Length == 0)
                return null;
            else
            {
                Dictionary<string, string> arguments = new Dictionary<string, string>();

                for (int i=0; i<args.Length; i++)
                {
                    // Check if we got a tag.
                    if (args[i].StartsWith("-"))
                    {
                        // Check if the next argument is a tag. If so, set the current tag to an empty string.
                        if (args.Length-1 < i+1 || args[i+1].StartsWith("-"))
                            arguments.Add(args[i].TrimStart('-'), "");
                        // Set the current tag to whatever follows it and adjust i accordingly.
                        else
                        {
                            arguments.Add(args[i].TrimStart('-'), args[i+1]);
                            i++;
                        }
                    }
                }

                return arguments;
            }
        }

        /// <summary>
        /// Small helper function to pause the console like in the command-line "pause" keyword.
        /// </summary>
        private static void Pause()
        {
            Console.WriteLine("Press any key to continue...");
            try
            {
                Console.ReadKey();
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("Failed to pause program, has console input been redirected?");
            }
        }

        /// <summary>
        /// Loads or creates a new JSON file containing information on excluded addon names.
        /// </summary>
        /// <param name="path">The path to the JSON file.</param>
        /// <returns>A list of the excluded addons.</returns>
        private static List<string> LoadExcludedAddons(string path)
        {
            List<string> excludedAddons = new List<string>();

            // Check if a file exists at the provided path.
            if (File.Exists(path))
            {
                // Read the JSON file and deserialize it.
                string jsonText = File.ReadAllText(path);
                excludedAddons.AddRange(JsonConvert.DeserializeObject<string[]>(jsonText));
            }
            else
            {
                // Create default array.
                string[] defaultExcludedAddons = new string[]
                {
                    ".git",
                    DEFAULT_MERGE_DIR
                };

                // Write the serialized default array.
                File.WriteAllText(path, JsonConvert.SerializeObject(defaultExcludedAddons));
                excludedAddons.AddRange(defaultExcludedAddons);
            }

            return excludedAddons;
        }

        /// <summary>
        /// Takes in an array of addons and checks them against a list of excluded addon names. Returns the finalized list.
        /// </summary>
        /// <param name="excludedAddons">List of excluded addons to remove from the addons list.</param>
        /// <param name="addons">Array of addon directories.</param>
        /// <returns></returns>
        private static List<string> FilterAddons(List<string> excludedAddons, string[] addons)
        {
            List<string> finalAddons = new List<string>();

            // Loop over every addon and check if it should be ignored.
            for (int i=0; i<addons.Length; i++)
            {
                if (excludedAddons.Contains(Path.GetFileName(addons[i])))
                    Console.WriteLine("Ignoring {0}...", addons[i]);
                else
                    finalAddons.Add(addons[i]);
            }

            return finalAddons;
        }

        /// <summary>
        /// Starts up the workers needed to execute the merging process.
        /// </summary>
        /// <param name="addons">List of addons to merge.</param>
        /// <param name="outputDirectory">The directory path to place the merged files in.</param>
        private static void MergeAddons(List<string> addons, string outputDirectory)
        {
            // Setup log directories.
            if (LOG)
            {
                Directory.CreateDirectory($"{STARTUP_PATH}\\temp");
                Directory.CreateDirectory($"{STARTUP_PATH}\\temp\\errors");
                Directory.CreateDirectory($"{STARTUP_PATH}\\temp\\conflicts");
            }

            // Calculate how many threads we can use and how many addons need to be spread between them.
            int numThreads = Math.Min((int)Math.Ceiling((double)addons.Count / ADDONS_PER_THREAD), MAX_THREADS);
            int addonsPerThread = Math.Max((int)Math.Ceiling((double)addons.Count / numThreads), ADDONS_PER_THREAD);
            Console.WriteLine("Starting up {0} worker(s)...", numThreads);

            // Create and start workers.
            Worker[] workers = new Worker[numThreads];
            for (int i = 0; i < numThreads; i++)
            {
                string[] addonPaths = addons.GetRange(0 + (i*addonsPerThread), Math.Min(addons.Count - (i*addonsPerThread), addonsPerThread)).ToArray();
                workers[i] = new Worker(i+1, outputDirectory, addonPaths, new WorkerOptions()
                {
                    canOverwriteConflicts = OVERWRITE_CONFLICTS,
                    canLog = LOG
                });

                workers[i].Start();
            }

            // Sleep the main thread temporarily and check if the processing threads have all finished.
            bool isFinished = false;
            while (!isFinished)
            {
                Thread.Sleep(50);

                for (int i=0; i<numThreads; i++)
                {
                    if (workers[i].IsAlive())
                        break;

                    if (i == numThreads-1)
                        isFinished = true;
                }
            }

            Console.WriteLine("Finished merging {0} addons into {1}!", addons.Count, outputDirectory);

            // Compile logs and delete temp directories/files.
            if (LOG)
            {
                Console.WriteLine("Compiling logs...");
                List<string> errorsLog = new List<string>();
                List<string> conflictsLog = new List<string>();

                // Build log files data.
                for (int i=0; i<numThreads; i++)
                {
                    string workerString = $"-- Worker {i+1} --";
                    errorsLog.Add(workerString);
                    errorsLog.AddRange(File.ReadAllLines(String.Format(Worker.logLocation, "errors", i+1)));
                    errorsLog.Add("");

                    conflictsLog.Add(workerString);
                    conflictsLog.AddRange(File.ReadLines(String.Format(Worker.logLocation, "conflicts", i+1)));
                    conflictsLog.Add("");
                }

                // Write files.
                File.WriteAllLines($"{STARTUP_PATH}\\errors.txt", errorsLog.ToArray());
                File.WriteAllLines($"{STARTUP_PATH}\\conflicts.txt", conflictsLog.ToArray());

                // Cleanup.
                Directory.Delete($"{STARTUP_PATH}\\temp", true);
            }

            // Finish with a pause.
            Pause();
        }
    }
}