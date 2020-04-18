using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
		public string Info => "Searches bing for images. The query could be a string or a path to an image for reverse visual search." + Environment.NewLine +
			" not interactive (-ni), azure key (-key), search query/file path (-q), count (-count), out file (-out),  " +
			" additional filters (-filters) in the format <key> : <value>, <key2> <value2>" +
			" The count doesn't affect the visual search results. They are always under 100. " +
			" Note that the Azure keys will probably be different for the text and image modes." +
			" API Ref: https://docs.microsoft.com/en-us/rest/api/cognitiveservices-bingsearch/bing-images-api-v7-reference";

		public int Run(RunArgs ra)
		{
			bool interactive = !ra.InArgs.ContainsKey("-ni");
			var outFile = string.Empty;
			var azureKey = string.Empty;
			var query = string.Empty;
			var count = 1;
			var F = new Dictionary<string, string>();
			var allFilters = string.Empty;

			if (interactive)
			{
				Utils.ReadString("Azure key: ", ref azureKey, true);
				Utils.ReadString("Search query: ", ref query, true);
				Utils.ReadString("Links file: ", ref outFile, true);
				Utils.ReadInt("Count: ", ref count, true);
				Utils.ReadString("Filters in the format \"<key> : <value>, <key2> <value2>\": ", ref allFilters);
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
				if (ra.InArgs.ContainsKey("-filters")) allFilters = ra.InArgs.GetFirstValue("-filters");
			}

			if (!string.IsNullOrEmpty(allFilters))
			{
				allFilters = allFilters.Replace('"', ' ');

				foreach (var f in allFilters.Split(','))
				{
					var trimmed = f.Trim();
					var kv = f.Split(':');

					if (kv.Length > 1)
					{
						var key = kv[0].Trim();
						var val = kv[1].Trim();

						if (!F.ContainsKey(key))
							F.Add(key, string.Empty);

						F[key] = val;
					}
				}
			}

			var URLs = new HashSet<string>();
			var PAGE = 100;

			if (File.Exists(query))
			{
				var CropArea = new CropArea(0.01, 0.99, 0.01, 0.99);
				var vinfo = new ImageInfo(cropArea: CropArea);
				var vreq = new VisualSearchRequest(imageInfo: vinfo);
				var visClient = new VisualSearchClient(new VisualCredentials(azureKey));
				var f = File.ReadAllBytes(query);

				using (var ms = new MemoryStream(f))
				{
					var R = visClient.Images.VisualSearchMethodAsync(image: ms, knowledgeRequest: vreq).Result;

					if (R != null)
					{
						var A = R.Tags
							.SelectMany(x => x.Actions.Where(a => a.ActionType == "VisualSearch"))
							.ToList();

						foreach (ImageModuleAction a in A)
							foreach (var c in a.Data.Value)
								URLs.Add(c.ContentUrl);
					}
				}
			}
			else
			{
				var imgClient = new ImageSearchClient(new ImageCredentials(azureKey));
				var pages = 0;

				while (URLs.Count < count)
				{
					var URLsCount = URLs.Count;
					var offset = PAGE * pages;
					var R = imgClient.Images.SearchAsync(
						query: query,
						count: PAGE,
						minWidth: F.ContainsKey("minWidth") ? long.Parse(F["minWidth"]) : default(long?),
						maxWidth: F.ContainsKey("maxWidth") ? long.Parse(F["maxWidth"]) : default(long?),
						minHeight: F.ContainsKey("minHeight") ? long.Parse(F["minHeight"]) : default(long?),
						maxHeight: F.ContainsKey("maxHeight") ? long.Parse(F["maxHeight"]) : default(long?),
						size: F.ContainsKey("size") ? F["size"] : null,
						minFileSize: F.ContainsKey("minFileSize") ? long.Parse(F["minFileSize"]) : default(long?),
						maxFileSize: F.ContainsKey("maxFileSize") ? long.Parse(F["maxFileSize"]) : default(long?),
						freshness: F.ContainsKey("freshness") ? F["freshness"] : null,
						aspect: F.ContainsKey("aspect") ? F["aspect"] : null,
						offset: offset).Result;

					if (R != null && R.Value.Count > 0)
					{
						var links = R.Value.Select(x => x.ContentUrl);

						foreach (var l in links)
							if (URLs.Count < count) URLs.Add(l);
							else break;

						if (URLsCount == URLs.Count) break;

						pages++;
					}
					else break;
				}
			}

			File.WriteAllLines(outFile, URLs.ToArray());
			string.Format("Name file saved as {0}", outFile).PrintLine();
			"Done.".PrintLine();

			return 0;
		}
	}
}
