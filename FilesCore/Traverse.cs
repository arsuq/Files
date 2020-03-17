using System;
using System.Diagnostics;
using System.IO;

namespace Utils.Files
{
	public class Traverse : IUtil
	{
		public string Name => "traverse";
		public string Info =>
			"Traverses all sub-folders of a given root and launches a process with the provided args. If specified replaces a template string" +
			"in the args with the current folder full path or name." + Environment.NewLine +
			"Args: root (-root), program to call (-proc), program args (-pargs) Put in quotes and add a single space before the pargs string. " +
			"current dir full path as arg name (-cdf),  current dir as arg (-cd), recursive (-rec)" + Environment.NewLine +
			"Example: files -p traverse -ni -cdf $$ -cd $ -root <path> -proc <files> -pargs \" -p move -ni -zpad 3 -src $$ -dest $$ -prf $-\"";


		public int Run(RunArgs ra)
		{
			bool interactive = !ra.InArgs.ContainsKey("-ni");
			DirectoryInfo root = null;
			string prog = null;
			string progArgs = null;
			string cdf = null;
			string cd = null;
			bool recursive = false;

			if (interactive)
			{
				string src = null;
				Utils.ReadString("Source dir (current): ", ref src);
				root = !string.IsNullOrEmpty(src) ? new DirectoryInfo(src) : ra.RootDir;
				Utils.ReadString("Program to run for each dir: ", ref prog, true);
				recursive = Utils.ReadWord("Recursive dir traversal? (y/*):", "y");

				Utils.ReadString("Program arguments: ", ref progArgs, true);
				Utils.ReadString("Argument string to be replaced by the full current dir path: ", ref cdf);
				Utils.ReadString("Argument string to be replaced by the current dir name: ", ref cd);
			}
			else
			{
				root = new DirectoryInfo(ra.InArgs.GetFirstValue("-root"));
				prog = ra.InArgs.GetFirstValue("-proc");
				progArgs = ra.InArgs.GetFirstValue("-pargs");
				if (ra.InArgs.ContainsKey("-cdf")) cdf = ra.InArgs.GetFirstValue("-cdf");
				if (ra.InArgs.ContainsKey("-cd")) cd = ra.InArgs.GetFirstValue("-cd");
				if (ra.InArgs.ContainsKey("-rec")) recursive = true;
			}

			foreach (var dir in root.EnumerateDirectories("*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
				try
				{
					var pargs = progArgs;
					if (!string.IsNullOrWhiteSpace(cdf)) pargs = pargs.Replace(cdf, "\"" + dir.FullName + "\"");
					if (!string.IsNullOrWhiteSpace(cd)) pargs = pargs.Replace(cd, dir.Name);

					var proc = new ProcessStartInfo(prog, pargs);

					$"Starting {prog} {pargs}".PrintLine();

					using (var process = Process.Start(proc))
						process.WaitForExit();
				}
				catch (Exception ex)
				{
					ex.Message.PrintLine(ConsoleColor.Red);
				}

			return 0;
		}
	}
}
