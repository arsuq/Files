using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Azure.CognitiveServices.Search.ImageSearch;
using Microsoft.Azure.CognitiveServices.Search.ImageSearch.Models;
using Microsoft.Azure.CognitiveServices.Search.VisualSearch;
using Microsoft.Azure.CognitiveServices.Search.VisualSearch.Models;
using ImageCredentials = Microsoft.Azure.CognitiveServices.Search.ImageSearch.ApiKeyServiceClientCredentials;
using VisualCredentials = Microsoft.Azure.CognitiveServices.Search.VisualSearch.ApiKeyServiceClientCredentials;
using VM = Microsoft.Azure.CognitiveServices.Search.VisualSearch.Models;

namespace Utils.Files
{
	public class Bing : IUtil
	{
		public string Name => "bing";
		public string Info => "Searches bing for images." + Environment.NewLine +
			" not interactive (-ni), azure key (-key), search query/file path (-q), count (-count), out file (-out)";

		public int Run(RunArgs ra)
		{
			bool interactive = !ra.InArgs.ContainsKey("-ni");
			var outFile = string.Empty;
			var azureKey = string.Empty;
			var query = string.Empty;
			var count = 1;

			if (interactive)
			{
				Utils.ReadString("Azure key: ", ref azureKey, true);
				Utils.ReadString("Search query: ", ref query, true);
				Utils.ReadString("Links file: ", ref outFile, true);
				Utils.ReadInt("Count file: ", ref count, true);
			}
			else
			{
				if (ra.InArgs.ContainsKey("-key")) azureKey = ra.InArgs.GetFirstValue("-key");
				else throw new ArgumentNullException("-key");
				if (ra.InArgs.ContainsKey("-q")) query = ra.InArgs.GetFirstValue("-q");
				else throw new ArgumentNullException("-q");
				if (ra.InArgs.ContainsKey("-out")) outFile = ra.InArgs.GetFirstValue("-out");
				else throw new ArgumentNullException("-out");
				if (ra.InArgs.ContainsKey("-count")) count = int.Parse(ra.InArgs.GetFirstValue("-count"));
				else throw new ArgumentNullException("-count");
			}

			var URLs = new HashSet<string>();
			var PAGE = 100;

			if (File.Exists(query))
			{
				var CropArea = new CropArea(0.1, 0.9, 0.1, 0.9);
				var vinfo = new ImageInfo(cropArea: CropArea);
				var vreq = new VisualSearchRequest(imageInfo: vinfo);
				var visClient = new VisualSearchClient(new VisualCredentials(azureKey));

				using (var stream = new FileStream(query, FileMode.Open))
				{
					var R = visClient.Images.VisualSearchMethodAsync(image: stream, knowledgeRequest: vreq).Result;

					while (URLs.Count < count)
					{
						var URLsCount = URLs.Count;
						var offset = URLs.Count / PAGE;

						if (R != null)
						{
							var A = R.Tags.SelectMany(x => x.Actions.Where(a => a.ActionType == "VisualSearch")).ToList();

							foreach (ImageModuleAction a in A)
								foreach (var c in a.Data.Value)
									URLs.Add(c.ContentUrl);

							if (URLsCount == URLs.Count) break;
						}
					}
				}
			}
			else
			{
				var imgClient = new ImageSearchClient(new ImageCredentials(azureKey));
				while (URLs.Count < count)
				{
					var URLsCount = URLs.Count;
					var offset = URLs.Count / PAGE;
					var R = imgClient.Images.SearchAsync(query: query, count: PAGE, offset: offset).Result;
					if (R != null)
					{
						var links = R.Value.Select(x => x.ContentUrl);

						foreach (var l in links)
							URLs.Add(l);

						if (URLsCount == URLs.Count) break;
					}
				}
			}

			File.WriteAllLines(outFile, URLs.ToArray());
			string.Format("Name file saved as {0}", outFile).PrintLine();
			"Done.".PrintLine();

			return 0;
		}
	}
}
