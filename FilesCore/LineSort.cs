using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Utils.Files
{
	public class LineSort : IUtil
	{
		public string Name => "lsort";
		public string Info => "Sorts the lines in a text file in ascending or descending order. " +
			"This will override the original file!" + Environment.NewLine +
			"Args: not interactive (-ni), text file (-tf) descending order [by default is asc] (-desc)";

		public int Run(RunArgs ra)
		{
			bool interactive = !ra.InArgs.ContainsKey("-ni");
			var allText = string.Empty;
			var tf = string.Empty;
			var desc = false;

			if (interactive)
			{
				Utils.ReadString("File: ", ref tf, true);
				desc = Utils.ReadWord("Sort in descending order? [by default is asc] (y/*): ", "y");
			}
			else
			{
				if (ra.InArgs.ContainsKey("-tf")) tf = ra.InArgs.GetFirstValue("-tf");
				else throw new ArgumentNullException("-tf");
				if (ra.InArgs.ContainsKey("-desc")) desc = true;

			}

			allText = File.ReadAllText(tf);
			var allLines = allText.Split(Environment.NewLine);
			var len = allLines.Length;

			Array.Sort(allLines);
			if (desc) Array.Reverse(allLines);
			
			File.Delete(tf);
			File.WriteAllLines(tf, allLines);

			"Done.".PrintLine();

			return 0;
		}
	}
}
