using System;
using System.IO;
using System.Threading;

namespace Tamewater.GMOD_AddonMerger
{
    class Worker
    {
        // Instance variables.
        private ProcessingThreadInfo info;
        private Thread workerThread;
        private WorkerOptions options;

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
        private static void ProcessThread(object infoObject)
        {
            // Convert object to the correct type.
            ProcessingThreadInfo info = (ProcessingThreadInfo)infoObject;

            // Locals.
            int processingThreadNum = info.processingThreadNum;
            string[] addons = info.addonPaths;
            string outputDirectory = info.outputDirectoryPath;

            string workerThreadString = $"[Worker Thread {processingThreadNum}]";

            // Process the addon folders.
            Console.WriteLine("{0} Started processing...", workerThreadString);
            Directory.CreateDirectory(outputDirectory);
            for (int i = 0; i < addons.Length; i++)
            {
                string addonPath = addons[i];
                Console.WriteLine("{0} Started processing addon: {1}", workerThreadString, addonPath);
                ProcessDirectory(info.instance, outputDirectory, addonPath);
                Console.WriteLine("{0} Finished processing addon: {1}", workerThreadString, addonPath);
            }
            Console.WriteLine("{0} Finished merging {1} addons into {2}", workerThreadString, addons.Length, outputDirectory);
        }

        /// <summary>
        /// Helper function to recursively copy each directory to the provided output location.
        /// </summary>
        /// <param name="instance">The instance of Worker that this thread is working within.</param>
        /// <param name="outputPath">The path to place the found files within.</param>
        /// <param name="directoryPath">The path of the directory to look for files in.</param>
        private static void ProcessDirectory(Worker instance, string outputPath, string directoryPath)
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
                    Console.Write("!---------------!\nFile exception occurred!\n{0}\n!---------------!", e.Message);
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
