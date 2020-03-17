using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Utils.Files
{
	public class Restore : IUtil
	{
		public string Name => "restore";
		public string Info => "Moves files matched by filename to a location listed a log file map." + Environment.NewLine +
			" not interactive (-ni), root dir (-src), log file (-map), search filter (-flt), not recursive (-nrec)" + Environment.NewLine +
			" copy [default is move] (-copy) ";

		public int Run(RunArgs ra)
		{
			bool interactive = !ra.InArgs.ContainsKey("-ni");
			var newSrc = string.Empty;
			var logfilePath = string.Empty;
			var copy = false;
			var recursive = true;

			if (interactive)
			{
				Utils.ReadString("Source dir [default is local]: ", ref newSrc);
				Utils.ReadString("search pattern (*.*): ", ref ra.State.SearchPattern);
				Utils.ReadString("Log file path: ", ref logfilePath, true);
				recursive = Utils.ReadWord("Recursive search? (y/*): ", "y");
				copy = Utils.ReadWord("Copy? [default is move] (y/*): ", "y");
			}
			else
			{
				if (ra.InArgs.ContainsKey("-map")) logfilePath = ra.InArgs.GetFirstValue("-map");
				else throw new ArgumentNullException("-map");
				if (ra.InArgs.ContainsKey("-src")) newSrc = ra.InArgs.GetFirstValue("-src");
				if (ra.InArgs.ContainsKey("-flt")) ra.State.SearchPattern = ra.InArgs.GetFirstValue("-flt");
				recursive = !ra.InArgs.ContainsKey("-nrec");
				copy = ra.InArgs.ContainsKey("-copy");
			}

			if (!string.IsNullOrEmpty(newSrc)) ra.ChangeRoot(newSrc);

			var paths = File.ReadAllLines(logfilePath);
			var so = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
			var allFiles = Directory.GetFiles(ra.RootDir.FullName, ra.State.SearchPattern, so);

			var origMap = new Dictionary<string, string>();
			var localMap = new Dictionary<string, string>();

			foreach (var path in paths)
			{
				var jn = Path.GetFileName(path);

				if (!origMap.ContainsKey(jn))
					origMap.Add(jn, path);
				else $"Log file name duplicate {jn}".Print(ConsoleColor.Yellow, null, true);
			}

			foreach (var path in allFiles)
			{
				var jn = Path.GetFileName(path);

				if (!localMap.ContainsKey(jn))
					localMap.Add(jn, path);
				else $"Local file name duplicate {jn}".Print(ConsoleColor.Yellow, null, true);
			}

			var list = new List<string>(localMap.Keys.Intersect(origMap.Keys));

			$"{list.Count} files match by name.".PrintLine();

			if (list.Count > 0)
			{
				if (interactive && Utils.ReadWord("See them? (y/*): ", "y"))
					foreach (var fn in list)
						$"{localMap[fn]} ->  {origMap[fn]}".PrintLine();

				if (!interactive || Utils.ReadWord("Restore? (y/*): ", "y"))
					foreach (var fn in list)
					{
						if (copy) File.Copy(localMap[fn], origMap[fn], true);
						else File.Move(localMap[fn], origMap[fn], true);
					}
				else "Aborting".PrintLine();
			}

			"Done.".PrintLine();

			return 0;
		}
	}
}
