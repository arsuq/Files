using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using System.Xml.Serialization;

namespace Utils.Files
{
	public class Duplicates : IUtil
	{
		public string Name => "duplicates";
		public string Info =>
			"Detects file duplicates in one or more folders by comparing sizes, names or data hashes." + Environment.NewLine +
			"There are extension and size filters as well as an option for partial hashing by skip/taking portions of the files.";

		public int Run(RunArgs ra)
		{
			var srcDirs = new List<DirectoryInfo>();
			var srcs = string.Empty;
			var skipLessThanSize = 1;
			var skipMoreThanSize = -1;
			var ignExt = string.Empty;
			var iExt = new List<string>();

			Utils.ReadString("Enter folders to search into, separated by semicolon: ", ref srcs, true);
			Utils.ReadString("Search pattern (*.*): ", ref ra.State.SearchPattern);
			var recursive = !Utils.ReadWord("Recursive search (default is yes)? (n/*): ", "n");
			Utils.ReadInt($"Skip if size < 1Kb: ", ref skipLessThanSize, false);
			Utils.ReadInt($"Skip if size > #Kb: ", ref skipMoreThanSize, false);
			Utils.ReadString("Skip extensions (.xyz): ", ref ignExt, false);

			if (!string.IsNullOrEmpty(ignExt))
				foreach (var ext in ignExt.Split(';'))
					iExt.Add(ext.Trim());

			var compHash = Utils.ReadWord("Compare file names (default) or MD5 hashes? (h/*): ", "h");
			var inParallel = 1;
			var useStreamReduction = false;
			var take = 4000;
			var skip = 10000;

			if (compHash)
			{
				Utils.ReadInt($"Concurrent readers (1-{Environment.ProcessorCount}): ", ref inParallel);
				"By default the hash is computed over the whole file.".PrintLine();
				"You can use skip and take parameters to read a portion of the file.".PrintLine();
				useStreamReduction = Utils.ReadWord("Do you want to use skip/take? (y/*): ", "y");

				if (useStreamReduction)
				{
					"The reader always starts with a TAKE (position 0).".PrintLine();
					Utils.ReadInt($"Take bytes ({take}): ", ref take, false);
					Utils.ReadInt($"Skip bytes ({skip}): ", ref skip, false);

					if (skip < 0 || take < 0) throw new ArgumentOutOfRangeException("Negative skip or take value.");
				}
			}

			if (inParallel < 1 || inParallel > Environment.ProcessorCount)
				throw new ArgumentOutOfRangeException("inParallel", $"The concurrent readers should be between 0 and {Environment.ProcessorCount}");

			foreach (var p in srcs.Split(';'))
				if (Directory.Exists(p))
					srcDirs.Add(new DirectoryInfo(p));
				else throw new DirectoryNotFoundException(p);// will throw if the path is invalid

			var All = new List<List<FileInfo>>();
			var searchOpt = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

			// Some subdirectories may be restricted and the GetFiles will throw with AccesDenied
			// so loop the individual folders separately.
			foreach (var d in srcDirs)
				try
				{
					foreach (var sd in d.EnumerateDirectories("*", searchOpt).Union(new DirectoryInfo[] { d }))
						try
						{
							var L = new List<FileInfo>();

							foreach (var file in sd.EnumerateFiles(ra.State.SearchPattern, SearchOption.TopDirectoryOnly))
							{
								if (iExt.Count > 0 && iExt.Contains(file.Extension)) continue;
								if (skipLessThanSize > 0 && file.Length < skipLessThanSize) continue;
								if (skipMoreThanSize > 0 && file.Length > skipMoreThanSize) continue;

								L.Add(file);
							}

							All.Add(L);
						}
						catch (Exception ex)
						{
							ex.Message.PrintLine(ConsoleColor.Red);
						}
				}
				catch (Exception ex)
				{
					ex.Message.PrintLine(ConsoleColor.Red);
				}

			var dict = new Dictionary<string, List<FileInfo>>();
			var sb = new StringBuilder();
			var totalFiles = 0;
			var totalFilesChecked = 0;

			// In all cases the length will be the same
			foreach (var src in All)
				foreach (var fi in src)
				{
					var key = $"{fi.Length}";
					if (!dict.ContainsKey(key))
						dict.Add(key, new List<FileInfo>());
					else totalFiles++;

					totalFilesChecked++;
					dict[key].Add(fi);
				}


			totalFiles += totalFiles;

			// Either compare the names or the data hashes
			var hashDict = new Dictionary<string, List<FileInfo>>();
			var totalDuplicates = 0;
			var counter = 0;
			var hashDictSpin = new SpinLock();

			$"{Environment.NewLine}Total files checked: {totalFilesChecked}".PrintLine();
			Console.CursorVisible = false;
			var cursorTop = Console.CursorTop;

			foreach (var kv in dict)
				if (kv.Value.Count > 1)
					if (compHash)
					{
						Console.SetCursorPosition(0, cursorTop);

						Parallel.For(0, kv.Value.Count, new ParallelOptions() { MaxDegreeOfParallelism = inParallel }, (i) =>
						{
							try
							{
								Stream stream = File.OpenRead(kv.Value[i].FullName);
								if (useStreamReduction) stream = new StreamReductor(stream, skip, take);

								using (var md5 = MD5.Create())
								using (stream)
								{
									$"{++counter}/{totalFiles} [{kv.Value[i].Length / 1000}]Kb file{i} = {kv.Value[i].FullName}".PrintLine(ConsoleColor.Yellow);

									var h = md5.ComputeHash(stream);
									var key = BitConverter.ToString(h);
									var acq = false;

									hashDictSpin.Enter(ref acq);

									if (acq)
									{
										if (!hashDict.ContainsKey(key)) hashDict.Add(key, new List<FileInfo>());
										else totalDuplicates++;

										hashDict[key].Add(kv.Value[i]);
										hashDictSpin.Exit();
									}
								}
							}
							catch (Exception ex)
							{
								ex.Message.PrintSysError();
								return;
							}
						});

						var cursorTopNow = Console.CursorTop;
						for (int top = cursorTop; top < cursorTopNow; top++)
							for (int left = 0; left < Console.WindowWidth; left++)
							{
								Console.SetCursorPosition(left, top);
								" ".Print();
							}
					}
					else
						foreach (var f in kv.Value)
						{
							var key = f.Name.ToLowerInvariant();

							if (!hashDict.ContainsKey(key))
								hashDict.Add(key, new List<FileInfo>());
							else totalDuplicates++;

							hashDict[key].Add(f);
							$"Comparing {++counter}/{totalFiles}".Print(ConsoleColor.Yellow);
							Console.SetCursorPosition(0, Console.CursorTop);
						}

			Console.SetCursorPosition(0, cursorTop);
			Console.CursorVisible = true;

			sb.AppendLine("Files with the same length and name/hash are grouped together.");
			sb.AppendLine();

			foreach (var kv in hashDict)
				if (kv.Value.Count > 1)
				{
					var size = kv.Value[0].Length / 1000;
					string lbl = size < 1 ? lbl = $"{kv.Value[0].Length}b" : $"{size}Kb";

					sb.AppendLine($"[{kv.Key}] {lbl}");
					foreach (var p in kv.Value)
						sb.AppendLine(p.FullName);

					sb.AppendLine();
				}


			$"There are {totalDuplicates} files with clones.".PrintLine();

			if (totalDuplicates > 0)
			{
				ra.Trace = Utils.ReadWord("Trace? (y/*): ", "y");

				if (ra.Trace) sb.ToString().PrintLine(ConsoleColor.Yellow);

				var opt = string.Empty;

				if (Utils.PickOption("Save results? (fdelete, xml, json, txt/*): ", ref opt, false, "fdelete", "xml", "json", "txt"))
				{
					var fn = string.Empty;
					var data = string.Empty;

					Utils.ReadString("Result file path: ", ref fn, true);

					if (opt == "txt") data = sb.ToString();
					else if (opt == "fdelete")
					{
						var toDel = new StringBuilder();

						foreach (var kv in hashDict)
							if (kv.Value.Count > 1)
								foreach (var f in kv.Value.Select(x => x.FullName).Skip(1))
									toDel.AppendLine(f);

						data = toDel.ToString();
					}
					else
					{
						var L = new List<Duplicate>();

						foreach (var kv in hashDict)
							if (kv.Value.Count > 1)
								L.Add(new Duplicate(kv.Key, kv.Value[0].Length, kv.Value.Select(x => x.FullName).ToArray()));

						if (opt == "json") data = L.ToJson();
						else data = L.ToXml();
					}

					File.WriteAllText(fn, data);
				}
			}

			return 0;
		}
	}

	public class Duplicate
	{
		public Duplicate() { }

		public Duplicate(string key, long size, string[] files)
		{
			Key = key;
			Files = files;
			Size = size;
		}

		[XmlAttribute]
		public string Key;

		[XmlAttribute]
		public long Size;

		[XmlElement("fp")]
		public string[] Files;
	}
}
