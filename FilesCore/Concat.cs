using System;
using System.IO;
using System.Text;

namespace Utils.Files
{
	public class Concat : IUtil
	{
		public string Name => "concat";
		public string Info => "Concatenates text files." + Environment.NewLine +
			"Args: not interactive (-ni), out-file (-out), paths map file [separated by new line] (-files) " +
			"new-line-separator (-fsep)";

		public int Run(RunArgs ra)
		{
			bool interactive = !ra.InArgs.ContainsKey("-ni");
			var allText = string.Empty;
			var outFile = string.Empty;
			var fileMap = string.Empty;
			var newLinefileSep = false;

			if (interactive)
			{
				Utils.ReadString("Out file path: ", ref outFile, true);
				Utils.ReadString("The file paths map: ", ref fileMap, true);

				var newLine = string.Empty;
				Utils.ReadString("Add new line before each file? (y/*):  ", ref newLine);
				newLinefileSep = newLine == "y";
			}
			else
			{
				if (ra.InArgs.ContainsKey("-out")) outFile = ra.InArgs.GetFirstValue("-out");
				else throw new ArgumentNullException("-out");

				if (ra.InArgs.ContainsKey("-files")) fileMap = ra.InArgs.GetFirstValue("-files");
				else throw new ArgumentNullException("-files");

				if (ra.InArgs.ContainsKey("-fsep")) newLinefileSep = true;
			}

			var sb = new StringBuilder();
			var F = File.ReadAllLines(fileMap);
			var counter = 0;


			foreach (var f in F)
			{
				var t = f.Trim();

				if (t.Length > 0)
				{
					if (!File.Exists(t)) throw new FileNotFoundException($"{t}");
					else
					{
						if (counter > 0 && newLinefileSep) sb.Append(Environment.NewLine);
						sb.Append(File.ReadAllText(t));
						counter++;
					}

				}
			}

			File.WriteAllText(outFile, sb.ToString());

			$"Done {counter} files concatenated.".PrintInfo();

			return 0;
		}

		public const char FILE_SPLITTER = ',';
	}
}
