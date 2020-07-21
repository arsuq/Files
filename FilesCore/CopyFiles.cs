using System;
using System.IO;

namespace Utils.Files
{
	public class CopyFiles : IUtil
	{
		public string Name => "fcopy";
		public string Info => "Copies or moves files listed in a text file. Each file path must be on a separate line." + Environment.NewLine +
			"Args: not interactive (-ni), map file (-f), destination dir or map file (-dst), move [default is copy] (-m), " +
			"quiet (do not report missing) (-q), override existing (-ovr) ";

		public int Run(RunArgs ra)
		{
			bool interactive = !ra.InArgs.ContainsKey("-ni");
			var tf = string.Empty;
			var dst = string.Empty;
			var srcText = string.Empty;
			var quiet = false;
			var move = false;
			var ovr = false;

			if (interactive)
			{
				Utils.ReadString("File: ", ref tf, true);
				srcText = File.ReadAllText(tf);
				Utils.ReadString("Destination dir: ", ref dst, true);
				var q = string.Empty;
				var m = string.Empty;
				move = Utils.ReadWord("Move? The default mode is copy (y/*): ", "y");
				quiet = Utils.ReadWord("Quiet mode (y/*): ", "y");
				ovr = Utils.ReadWord("Override existing files (y/*): ", "y");
			}
			else
			{
				if (ra.InArgs.ContainsKey("-f")) srcText = File.ReadAllText(ra.InArgs.GetFirstValue("-f"));
				else throw new ArgumentNullException("-f");

				if (ra.InArgs.ContainsKey("-dst")) dst = ra.InArgs.GetFirstValue("-dst");
				else throw new ArgumentNullException("-dst");

				if (ra.InArgs.ContainsKey("-q")) quiet = true;
				if (ra.InArgs.ContainsKey("-m")) move = true;
				if (ra.InArgs.ContainsKey("-ovr")) ovr = true;
			}

			var counter = 0;
			var srcLines = srcText.Split(Environment.NewLine);

			// If dst is file like -f they must match in lines
			if (File.Exists(dst))
			{
				var dstText = File.ReadAllText(dst);
				var dstLines = dstText.Split(Environment.NewLine);

				if (dstLines.Length != dstLines.Length)
					throw new Exception("The source and destination do not match in length.");

				for (int i = 0; i < srcLines.Length; i++)
				{
					var from = srcLines[i];
					var to = dstLines[i];

					if (from.Length > 0)
						if (File.Exists(from))
						{
							var todir = Path.GetDirectoryName(to);
							if (!Directory.Exists(todir)) Directory.CreateDirectory(todir);

							if (File.Exists(to) && ovr) File.Delete(to);
							if (!File.Exists(to))
							{
								if (move) File.Move(from, to);
								else File.Copy(from, to);
								counter++;
							}
							else if (!quiet) $"File already exists: {to}".PrintLine();
						}
						else if (!quiet) $"File not found: {from}".PrintLine();
				}
			}
			else
			{
				if (!Directory.Exists(dst)) Directory.CreateDirectory(dst);

				foreach (var line in srcLines)
					if (line.Length > 0)
						if (File.Exists(line))
						{
							var fn = Path.GetFileName(line);
							var to = Path.Combine(dst, fn);

							if (File.Exists(to) && ovr) File.Delete(to);
							if (!File.Exists(to))
							{
								if (move) File.Move(line, to);
								else File.Copy(line, to);
								counter++;
							}
							else if (!quiet) $"File already exists: {to}".PrintLine();
						}
						else if (!quiet) $"File not found: {line}".PrintLine();
			}

			var com = move ? "moved" : "copied";
			$"Done, {counter} files were {com}.".PrintLine();

			return 0;
		}
	}
}
