using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Utils.Files
{
	public class Log : IUtil
	{
		public string Name => "log";
		public string Info => "Creates a file with paths (log), which can be used to move files (restore)." + Environment.NewLine +
			" not interactive (-ni), root dir (-src), search filter [*.*] (-flt), " + Environment.NewLine +
			" not recursive (-nrec), not full paths (-nfp), out file (-out)";

		public int Run(RunArgs ra)
		{
			bool interactive = !ra.InArgs.ContainsKey("-ni");
			var newSrc = string.Empty;
			ra.State.SearchPattern = "*.*";
			var currentDirOnly = false;
			var justNames = false;
			var logOutFile = string.Empty;

			if (interactive)
			{
				Utils.ReadString("Source dir [default is local]: ", ref newSrc);
				Utils.ReadString("search pattern (*.*): ", ref ra.State.SearchPattern);
				currentDirOnly = Utils.ReadWord("Recursive? [default is yes] (n/*): ", "n");
				justNames = Utils.ReadWord("Log full paths? [default is yes] (n/*): ", "n");
			}
			else
			{
				if (ra.InArgs.ContainsKey("-src")) newSrc = ra.InArgs.GetFirstValue("-src");
				if (ra.InArgs.ContainsKey("-flt")) ra.State.SearchPattern = ra.InArgs.GetFirstValue("-flt");
				currentDirOnly = ra.InArgs.ContainsKey("-nrec");
				justNames = ra.InArgs.ContainsKey("-nfp");
				if (ra.InArgs.ContainsKey("-out")) logOutFile = ra.InArgs.GetFirstValue("-out");
				else throw new ArgumentNullException("-out");
			}

			if (!string.IsNullOrEmpty(newSrc)) ra.ChangeRoot(newSrc);

			var F = new List<string>();
			foreach (var f in ra.RootDir.GetFiles(ra.State.SearchPattern,
				currentDirOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories))
				F.Add(justNames ? f.Name : f.FullName);

			if (interactive)
			{
				string.Format("Found {0} matching files.", F.Count).PrintLine();
			
				if (F.Count > 0)
				{
					if (Utils.ReadWord("See them? (y/*): ", "y"))
						foreach (var f in F)
							f.PrintLine(ConsoleColor.Yellow);

					"Save as (file path): ".Print();
					logOutFile = Path.GetFullPath(Console.ReadLine());
				}
			}

			File.WriteAllLines(logOutFile, F.ToArray());
			string.Format("Name file saved as {0}", logOutFile).PrintLine();

			"Done.".PrintLine();

			return 0;
		}
	}
}
