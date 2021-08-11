using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Utils.Files
{
	public class Flip : IUtil
	{
		public string Name => "flip";
		public string Info =>
			"Flips images from a map file or the current dir and saves the results in destination dir. " + Environment.NewLine +
			"Args: not interactive (-ni), map file [paths] (-map), out-dir (-out), mode (-mode) {h,v,hv,r90} " + Environment.NewLine +
			" [horizontal, vertical, both], search pattern [*.jpg] (-sp), result file prefix [opt] (-prf), suffix [opt] (-sfx) ";

		public int Run(RunArgs ra)
		{
			interactive = !ra.InArgs.ContainsKey("-ni");

			if (interactive)
			{
				Utils.ReadString("Images search pattern (*.jpg by default): ", ref sp, false);
				Utils.ReadString("Images map file: ", ref fmap, false);

				if (!string.IsNullOrEmpty(fmap)) paths = File.ReadAllText(fmap).Split(Environment.NewLine);
				else
				{
					"No map file was provided, will use the current dir as a source.".PrintLine();
					paths = Directory.GetFiles(ra.RootDir.FullName, sp);
				}

				if (paths != null && paths.Length > 0)
				{
					Utils.ReadString("Destination folder: ", ref dest, true);
					Utils.ReadString("Mode [h, v, hv or r90]: ", ref mode, true);

					if (Array.IndexOf(MODES, mode) < 0) throw new ArgumentNullException("-mode", "Incorrect mode");
					Utils.ReadString("Result image filename prefix [optional]: ", ref prf, false);
					Utils.ReadString("Result image filename suffix [optional]: ", ref sfx, false);
				}
				else throw new ArgumentException("", $"There are no images in {ra.RootDir.FullName}");
			}
			else
			{
				if (ra.InArgs.ContainsKey("-out")) dest = ra.InArgs.GetFirstValue("-out");
				else throw new ArgumentNullException("-out");

				if (ra.InArgs.ContainsKey("-mode"))
				{
					mode = ra.InArgs.GetFirstValue("-mode");

					if (Array.IndexOf(MODES, mode) < 0)
						throw new ArgumentException("-mode", "Incorrect mode.");
				}
				else throw new ArgumentNullException("-mode");

				if (ra.InArgs.ContainsKey("-sp")) sp = ra.InArgs.GetFirstValue("-sp");
				if (ra.InArgs.ContainsKey("-map")) paths = File.ReadAllText(ra.InArgs.GetFirstValue("-map")).Split(Environment.NewLine);
				else
				{
					"No map file was provided, will use the current dir as a source.".PrintLine();
					paths = Directory.GetFiles(ra.RootDir.FullName, sp);
				}

				if (paths == null || paths.Length < 1)
					throw new ArgumentException("", $"There are no images in {ra.RootDir.FullName}");

				if (ra.InArgs.ContainsKey("-prf")) prf = ra.InArgs.GetFirstValue("-prf");
				if (ra.InArgs.ContainsKey("-sfx")) prf = ra.InArgs.GetFirstValue("-sfx");
			}

			if (string.IsNullOrEmpty(prf)) prf = string.Empty;
			if (string.IsNullOrEmpty(sfx)) sfx = string.Empty;

			if (!Directory.Exists(dest)) Directory.CreateDirectory(dest);

			flip();

			return 0;
		}

		void flip()
		{
			"".PrintLine();

			var ct = Console.CursorTop;
			var cl = Console.CursorLeft;
			var processed = 0;

			Task.Run(async () =>
			{
				var p = Volatile.Read(ref processed);

				while (p < paths.Length)
				{
					await Task.Delay(200);

					p = Volatile.Read(ref processed);

					if (!Utils.SuppressPrint)
					{
						Console.SetCursorPosition(cl, ct);
						$"Processed {p}/{paths.Length}".Print();
					}
				}
			});

			Parallel.For(0, paths.Length, (i) =>
			{
				var path = paths[i].Trim();
				if (string.IsNullOrEmpty(path)) return;

				var file = new FileInfo(path);
				if (!file.Exists) return;

				using (var bmp = new Bitmap(file.FullName))
				{
					switch (mode)
					{
						case "h": bmp.RotateFlip(RotateFlipType.RotateNoneFlipX); break;
						case "v": bmp.RotateFlip(RotateFlipType.RotateNoneFlipY); break;
						case "hv": bmp.RotateFlip(RotateFlipType.RotateNoneFlipXY); break;
						case "r90": bmp.RotateFlip(RotateFlipType.Rotate90FlipNone); break;
						default: return;
					}

					var justName = Path.GetFileNameWithoutExtension(file.Name);
					var newName = $"{prf}{justName}{sfx}{file.Extension}";
					var newPath = Path.Combine(dest, newName);

					bmp.Save(newPath);
					Interlocked.Increment(ref processed);
				}
			});

			Interlocked.Exchange(ref processed, paths.Length);

			"".PrintLine();
			$"Done flipping.".PrintLine();
		}

		bool interactive;
		string fmap, dest, prf, sfx, mode;
		string sp = "*.jpg";
		string[] paths = null;
		readonly string[] MODES = new string[] { "h", "v", "hv", "r90" };
	}
}
