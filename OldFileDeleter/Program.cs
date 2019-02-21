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
			int numToDelete = 10;

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
				else
				{
					Console.Error.WriteLine("Unrecognised argument {0}", arg);
				}
			}

			Console.WriteLine("Deleting {1} files from \"{0}\"", rootDir, numToDelete);
			Console.WriteLine("Getting file list...");

			var rootDirInfo = new DirectoryInfo(rootDir);
			var allFileInfos = rootDirInfo.GetFiles("*.*", SearchOption.AllDirectories);

			Console.WriteLine("Sorting...");

			Array.Sort(allFileInfos, delegate (FileInfo a, FileInfo b)
			{
				return a.LastWriteTimeUtc.Ticks.CompareTo(b.LastWriteTimeUtc.Ticks);
			});

			Console.WriteLine("Deleting Files:");
			for(int i = 0; i < numToDelete; i++)
			{
				Console.WriteLine("    {0} {1}", allFileInfos[i].LastWriteTimeUtc.ToString(), allFileInfos[i].FullName);
				File.Delete(allFileInfos[i].FullName);
			}

			DateTime endtime = DateTime.Now;
			TimeSpan total = endtime - starttime;
			Console.WriteLine("Done in {0}", total);
		}
	}
}
