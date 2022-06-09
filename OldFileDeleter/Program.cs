using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OldFileDeleter
{
    [DebuggerDisplay("{Name}")]
    class FileInfo
    {
        public string Name;
        public string FullPath;
        public long FileSize;
        public DateTime LastAccessed;
    }

	class Program
	{
		static void Main(string[] args)
		{
			DateTime starttime = DateTime.Now;

            string rootDir = "";
            int numToDelete = -1;
            long targetSizeBytes = -1;
            List<string> ignoreFiles = new List<string>();

            int numArgs = args.Count();
			int argsParsed = 0;
			while(argsParsed != numArgs)
			{
				string arg = args[argsParsed++];
				if(arg == "-dir")
				{
					rootDir = args[argsParsed++];
				}
				else if(arg == "-ignore")
                {
					ignoreFiles.Add(args[argsParsed++]);
                }
				else if(arg == "-num")
				{
					numToDelete = int.Parse(args[argsParsed++]);
				}
				else if(arg == "-targetsizekb")
                {
					targetSizeBytes = long.Parse(args[argsParsed++]) * 1024;
                }
                else if (arg == "-targetsizemb")
                {
                    targetSizeBytes = long.Parse(args[argsParsed++]) * 1024 * 1024;
                }
                else if (arg == "-targetsizegb")
                {
                    targetSizeBytes = long.Parse(args[argsParsed++]) * 1024 * 1024 * 1024;
                }
                else
				{
					Console.Error.WriteLine("Unrecognised argument {0}", arg);
					return;
				}
			}

			Console.WriteLine("Getting file list...");
            var rootDirInfo = new DirectoryInfo(rootDir);
            List<FileInfo> AllFileInfos = new List<FileInfo>();
            var EnumeratedFiles = rootDirInfo.EnumerateFiles("*.*", SearchOption.AllDirectories);
            long TotalFileSize = 0;
            foreach(var EnumeratedFile in EnumeratedFiles)
            {
                if (ignoreFiles.Contains(EnumeratedFile.Name)) continue;

                TotalFileSize += EnumeratedFile.Length;

                FileInfo fileInfo = new FileInfo();
                fileInfo.Name = EnumeratedFile.Name;
                fileInfo.FullPath = EnumeratedFile.FullName;
                fileInfo.FileSize = EnumeratedFile.Length;
                fileInfo.LastAccessed = EnumeratedFile.LastAccessTimeUtc;
                AllFileInfos.Add(fileInfo);
            }

            Console.WriteLine("Sorting...");
            AllFileInfos.Sort(delegate (FileInfo a, FileInfo b)
            {
                return a.LastAccessed.Ticks.CompareTo(b.LastAccessed.Ticks);
            });

            Console.WriteLine("Deciding what to delete...");
            List<FileInfo> FilesToDelete = new List<FileInfo>();

            if(targetSizeBytes > 0)
            {
                long SizeRemainingToDelete = TotalFileSize - targetSizeBytes;
                numToDelete = 0;
				while(SizeRemainingToDelete > 0 && AllFileInfos.Count > numToDelete)
                {
                    SizeRemainingToDelete -= AllFileInfos[numToDelete].FileSize;
                    numToDelete++;
                }
            }

            if (numToDelete > 0)
            {
                numToDelete = Math.Min(numToDelete, AllFileInfos.Count);
                FilesToDelete = AllFileInfos.GetRange(0, numToDelete);
            }

            Console.WriteLine("Deleting {1}/{2} files from \"{0}\"", rootDir, FilesToDelete.Count, AllFileInfos.Count);
            foreach(var File in FilesToDelete)
            {
                try
                {
                    System.IO.File.Delete(File.FullPath);
                }
                catch (Exception e)
                {
                    Console.WriteLine("{0}: ", File.Name, e.Message);
                }
                System.IO.File.Delete(File.FullPath);
            };

			DateTime endtime = DateTime.Now;
			TimeSpan total = endtime - starttime;
			Console.WriteLine("Done in {0}", total);
		}
	}
}
