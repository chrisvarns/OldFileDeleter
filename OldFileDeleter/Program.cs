using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OldFileDeleter
{
	class Program
	{
		static void Main(string[] args)
		{
			DateTime starttime = DateTime.Now;

			string rootDir = "";
			int numToDelete = -1;
			long targetSizeBytes = -1;

			int numArgs = args.Count();
			int argsParsed = 0;
			while(argsParsed != numArgs)
			{
				string arg = args[argsParsed++];
				if(arg == "-dir")
				{
					rootDir = args[argsParsed++];
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
            var allFileInfos = rootDirInfo.GetFiles("*.*", SearchOption.AllDirectories);

            Console.WriteLine("Sorting...");
            Array.Sort(allFileInfos, delegate (FileInfo a, FileInfo b)
            {
                return a.LastAccessTime.Ticks.CompareTo(b.LastAccessTime.Ticks);
            });

            List<FileInfo> FilesToDelete = new List<FileInfo>();
			if(numToDelete > 0)
            {
                for (int i = 0; i < numToDelete && i < allFileInfos.Length; i++)
                {
					FilesToDelete.Add(allFileInfos[i]);
                }
            }
            else if(targetSizeBytes > 0)
            {
				long totalSizeBytes = allFileInfos.Sum(x => x.Length);
				int lastDeletedIdx = -1;
				while(totalSizeBytes > targetSizeBytes && allFileInfos.Length != FilesToDelete.Count)
                {
					++lastDeletedIdx;
					totalSizeBytes -= targetSizeBytes;
					FilesToDelete.Add(allFileInfos[lastDeletedIdx]);
                }
            }
            else
            {
				Console.WriteLine("No num or target size specified");
			}

            Console.WriteLine("Deleting {1}/{2} files from \"{0}\"", rootDir, FilesToDelete.Count, allFileInfos.Length);
			foreach (FileInfo File in FilesToDelete)
            {
                Console.WriteLine("    {0} {1}", File.LastAccessTimeUtc.ToString(), File.FullName);
                File.Delete();
            }

			DateTime endtime = DateTime.Now;
			TimeSpan total = endtime - starttime;
			Console.WriteLine("Done in {0}", total);
		}
	}
}
