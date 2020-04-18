using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Utils.Files
{
	public class Take : IUtil
	{
		public string Name => "take";
		public string Info =>
			"Copies or moves n random files from the current folder into a destination folder." + Environment.NewLine +
			"Args: not interactive (-ni), source dir [default is current] (-src), destination dir (-dest), recursive[y/*] (-r)," +
			"search pattern [*.*] (-sp), taken files prefix (-prf), mirror dir tree (-mt), take count (-take) move [default is copy mode] (-move)" +
			"ignore files smaller than [in KB] (-ist). ignore files bigger than [in KB] (-ibt), full-path regex [Ex: (?:dirA|dirB)] (-reg) ";

		public int Run(RunArgs ra)
		{
			bool interactive = !ra.InArgs.ContainsKey("-ni");
			var copy = true;
			var recursive = SearchOption.TopDirectoryOnly;
			var mirrorDir = false;
			var ignoreST = 0;
			var ignoreBT = int.MaxValue / 1000;
			var regx = string.Empty;

			if (interactive)
			{
				var src = string.Empty;
				Utils.ReadString("Source dir: ", ref src);
				if (!string.IsNullOrEmpty(src)) ra.RootDir = new DirectoryInfo(src);
				Utils.ReadString("Destination dir: ", ref ra.State.DestinationDir, true);
				Utils.ReadString("Search pattern (*.*): ", ref ra.State.SearchPattern);
				Utils.ReadInt("Ignore files smaller than (in KB): ", ref ignoreST);
				Utils.ReadInt("Ignore files bigger than (in KB): ", ref ignoreBT);
				Utils.ReadString("Path matching regex: ", ref regx);
				Utils.ReadString("Taken files prefix: ", ref ra.State.Prefix);
				if (Utils.ReadWord("Recursive? (y/*): ", "y")) recursive = SearchOption.AllDirectories;
				if (recursive == SearchOption.AllDirectories && Utils.ReadWord("Mirror dir tree? (y/*): ", "y")) mirrorDir = true;
				copy = !Utils.ReadWord("Move? (y/*): ", "y");
			}
			else
			{
				if (ra.InArgs.ContainsKey("-src")) ra.RootDir = new DirectoryInfo(ra.InArgs.GetFirstValue("-src"));
				ra.State.DestinationDir = ra.InArgs.GetFirstValue("-dest");
				if (ra.InArgs.ContainsKey("-sp")) ra.State.SearchPattern = ra.InArgs.GetFirstValue("-sp");
				if (ra.InArgs.ContainsKey("-prf")) ra.State.Prefix = ra.InArgs.GetFirstValue("-prf");
				ra.State.Take = int.Parse(ra.InArgs.GetFirstValue("-take"));
				if (ra.InArgs.ContainsKey("-r"))
				{
					recursive = SearchOption.AllDirectories;
					if (ra.InArgs.ContainsKey("-mt")) mirrorDir = true;
				}

				if (ra.InArgs.ContainsKey("-ist")) ignoreST = int.Parse(ra.InArgs.GetFirstValue("-ist"));
				if (ra.InArgs.ContainsKey("-ibt")) ignoreBT = int.Parse(ra.InArgs.GetFirstValue("-ibt"));
				if (ra.InArgs.ContainsKey("-reg")) regx = ra.InArgs.GetFirstValue("-reg");
				if (ra.InArgs.ContainsKey("-move")) copy = false;
			}

			if (ignoreST > ignoreBT || ignoreBT < 1 || ignoreST < 0) throw new ArgumentException();

			ignoreST *= 1000;
			ignoreBT *= 1000;

			if (!Directory.Exists(ra.State.DestinationDir))
				Directory.CreateDirectory(ra.State.DestinationDir);

			ra.State.Files = ra.RootDir.GetFiles(ra.State.SearchPattern, recursive)
				.Where(x => x.Length > ignoreST && x.Length < ignoreBT && (string.IsNullOrEmpty(regx) || Regex.IsMatch(x.FullName, regx)))
				.Select(x => x).ToArray();


			if (interactive) Utils.ReadInt(string.Format("There are {0} matches. take (0): ", ra.State.Files.Length), ref ra.State.Take);

			ra.Trace = interactive ? Utils.ReadWord("Trace first? (y/*): ", "y") : true;
			List<int> R = Utils.Randomize(ra.State.Files.Length);

			if (ra.State.Files.Length < ra.State.Take)
				ra.State.Take = ra.State.Files.Length;

			if (ra.Trace)
			{
				for (var i = 0; i < ra.State.Take; i++)
				{
					var j = R[i];
					var f = ra.State.Files[j];
					if (f.FullName == ra.Me) continue;
					var newname = mirrorDir ?
						Path.Combine(
							f.Directory.FullName.Replace(
								ra.RootDir.FullName, string.Empty),
								ra.State.Prefix + f.Name)
						.TrimStart(Path.DirectorySeparatorChar) :
						string.Format("{0}{1}", ra.State.Prefix, f.Name);

					string.Format("{0} -->{1}{2}{1}", f.FullName, Environment.NewLine, Path.Combine(ra.State.DestinationDir, newname))
						.PrintLine(ConsoleColor.Yellow);
				}
			}

			if (!interactive || (interactive && Utils.ReadWord("Take? (y/*): ", "y")))
			{
				var FI = new List<FileInfo>();
				for (var i = 0; i < ra.State.Take; i++)
				{
					var j = R[i];
					var f = ra.State.Files[j];
					var newname = mirrorDir ?
						Path.Combine(
							f.Directory.FullName.Replace(
								ra.RootDir.FullName, string.Empty),
								ra.State.Prefix + f.Name)
						.TrimStart(Path.DirectorySeparatorChar) :
						string.Format("{0}{1}", ra.State.Prefix, f.Name);

					var newpath = Path.Combine(ra.State.DestinationDir, newname);

					if (!Directory.Exists(newpath))
						Directory.CreateDirectory(Path.GetDirectoryName(newpath));

					if (copy) File.Copy(f.FullName, newpath);
					else File.Move(f.FullName, newpath);

					FI.Add(new FileInfo(newpath));
				}
				ra.State.Files = FI.ToArray();

				"".PrintLine();
				var mc = copy ? "copied" : "moved";
				$"Done - {ra.State.Take} files {mc}.".PrintLine();
			}
			else "Aborting take.".PrintLine();

			return 0;
		}
	}
}
