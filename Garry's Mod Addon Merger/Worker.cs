using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Tamewater.GMOD_AddonMerger
{
    class Worker
    {
        // Globals.
        public static string logLocation = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) + "\\temp\\{0}\\{1}.dat";

        // Instance variables.
        private ProcessingThreadInfo info;
        private Thread workerThread;
        private WorkerOptions options;
        private List<string> errors;
        private List<string> conflicts;

        /// <summary>
        /// Creates a new instance of Worker. This is used to handle a processing thread.
        /// </summary>
        /// <param name="workerNum">The ID of the worker.</param>
        /// <param name="outputDirectory">The directory the thread should be outputting to.</param>
        /// <param name="addons">The array of addon directories the thread will be working with.</param>
        /// <param name="options">Struct of configurable options for the thread.</param>
        public Worker(int workerNum, string outputDirectory, string[] addons, WorkerOptions options)
        {
            // Create the info struct.
            info = new ProcessingThreadInfo()
            {
                instance = this,
                processingThreadNum = workerNum,
                outputDirectoryPath = outputDirectory,
                addonPaths = addons
            };
            // Assign the options.
            this.options = options;

            // Create the worker thread.
            workerThread = new Thread(new ParameterizedThreadStart(ProcessThread));

            // Instantiate list objects if we are logging.
            if (options.canLog)
            {
                errors = new List<string>();
                conflicts = new List<string>();
            }
        }

        /// <summary>
        /// Wrapper function for Thread.Start.
        /// </summary>
        public void Start()
        {
            workerThread.Start(info);
        }

        /// <summary>
        /// Wrapper function for the Thread.IsAlive parameter.
        /// </summary>
        /// <returns>Whether the thread this worker is using is still alive.</returns>
        public bool IsAlive()
        {
            return workerThread.IsAlive;
        }

        /// <summary>
        /// The function that each thread will run. Will process each addon it has been provided.
        /// </summary>
        /// <param name="infoObject">Should be an instance of ProcessingThreadInfo, contains the information the thread needs to execute with.</param>
        private void ProcessThread(object infoObject)
        {
            // Convert object to the correct type.
            ProcessingThreadInfo info = (ProcessingThreadInfo)infoObject;

            // Locals.
            Worker instance = info.instance;
            int processingThreadNum = info.processingThreadNum;
            string[] addons = info.addonPaths;
            string outputDirectory = info.outputDirectoryPath;

            string workerThreadString = $"[Worker Thread {processingThreadNum}]";

            // Process the addon folders.
            Console.WriteLine("{0} Started processing...", workerThreadString);
            Directory.CreateDirectory(outputDirectory);
            for (int i=0; i<addons.Length; i++)
            {
                string addonPath = addons[i];
                Console.WriteLine("{0} Started processing addon: {1}", workerThreadString, addonPath);
                ProcessDirectory(instance, outputDirectory, addonPath);
                Console.WriteLine("{0} Finished processing addon: {1}", workerThreadString, addonPath);
            }
            Console.WriteLine("{0} Finished merging {1} addons into {2}", workerThreadString, addons.Length, outputDirectory);

            // Write logs to temp files.
            if (instance.options.canLog)
            {
                Console.WriteLine("{0} Writing logs to file...", workerThreadString);

                // Format paths
                string errorPath = String.Format(logLocation, "errors", processingThreadNum);
                string conflictPath = String.Format(logLocation, "conflicts", processingThreadNum);
                // Write errors
                if (instance.errors.Count != 0)
                    File.WriteAllLines(errorPath, instance.errors.ToArray());
                else
                    File.WriteAllLines(errorPath, new string[] { "No Errors!" });

                // Write conflicts
                if (instance.conflicts.Count != 0)
                    File.WriteAllLines(conflictPath, instance.conflicts.ToArray());
                else
                    File.WriteAllLines(conflictPath, new string[] { "No Conflicts!" });
            }

            Console.WriteLine("{0} Worker finished.", workerThreadString);
        }

        /// <summary>
        /// Helper function to recursively copy each directory to the provided output location.
        /// </summary>
        /// <param name="instance">The instance of Worker that this thread is working within.</param>
        /// <param name="outputPath">The path to place the found files within.</param>
        /// <param name="directoryPath">The path of the directory to look for files in.</param>
        private void ProcessDirectory(Worker instance, string outputPath, string directoryPath)
        {
            // Get info on the  directory we're viewing.
            DirectoryInfo dirInfo = new DirectoryInfo(directoryPath);

            // Copy each file to the output.
            foreach (FileInfo file in dirInfo.GetFiles())
            {
                try
                {
                    file.CopyTo($"{outputPath}\\{file.Name}", instance.options.canOverwriteConflicts);
                }
                catch (IOException e)
                {
                    // Log the failure.
                    if (instance.options.canLog)
                    {
                        // Conflict.
                        if (e.Message.EndsWith("already exists."))
                            instance.conflicts.Add(file.FullName);
                        // Some sort of error.
                        else
                            instance.errors.Add(e.Message);
                    }
                }
            }

            // Recursively copy each directory.
            foreach (DirectoryInfo directory in dirInfo.GetDirectories())
            {
                Directory.CreateDirectory($"{outputPath}\\{directory.Name}");
                ProcessDirectory(instance, $"{outputPath}\\{directory.Name}", directory.FullName);
            }
        }
    }

    /// <summary>
    /// Struct to hold the information that the processing threads need.
    /// </summary>
    struct ProcessingThreadInfo
    {
        // The instance of the worker object.
        public Worker instance;
        // Identifier for each thread in console output.
        public int processingThreadNum;
        // The directory that the addons should be merged into.
        public string outputDirectoryPath;
        // The array of addons it is responsible for merging.
        public string[] addonPaths;
    }

    /// <summary>
    /// Struct to hold configuration options for a worker.
    /// </summary>
    struct WorkerOptions
    {
        // Flag for whether files can be overwritten.
        public bool canOverwriteConflicts;
        // Flag for whether errors and conflicts can be logged.
        public bool canLog;
    }
}
