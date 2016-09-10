<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\Microsoft.Build.Framework.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\Microsoft.Build.Tasks.v4.0.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\Microsoft.Build.Utilities.v4.0.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.ComponentModel.DataAnnotations.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Configuration.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Design.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.DirectoryServices.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.DirectoryServices.Protocols.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.EnterpriseServices.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Net.Http.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Runtime.Caching.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Security.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.ServiceProcess.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.ApplicationServices.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.RegularExpressions.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.Services.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Windows.Forms.dll</Reference>
  <NuGetReference>HtmlAgilityPack</NuGetReference>
  <NuGetReference>Humanizer</NuGetReference>
  <NuGetReference>Newtonsoft.Json</NuGetReference>
  <Namespace>HtmlAgilityPack</Namespace>
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Web</Namespace>
</Query>

HttpClient c = new HttpClient();

async Task Main()
{
	// Specify a list of card names to retrieve data from the wiki for.
	var cards = File.ReadAllLines(@"c:\temp\empires.txt");
	//cards = new string[] { "Banquet" };
	
	foreach (string card in cards.Skip(0).Take(100))
	{
		//card.Dump("Name");
		string uri = $"http://wiki.dominionstrategy.com/api.php?action=parse&page={card}&prop=wikitext&format=xml";
		var doc = await c.GetXDocumentAsync(uri);
		//doc.ToString().Dump();
		string wikiText = doc.XPathSelectElement("/api/parse/wikitext[1]").Value;
				
		string name = wikiText.Match("\\|\\s*name = (.*)");
		string cost1 = wikiText.Match("\\|\\s*cost = (.*)");
		string cost2 = wikiText.Match("\\|\\s*cost2 = (.*)");
		if (!string.IsNullOrWhiteSpace(cost2)) cost2 = cost2 + "D";
		string cost = string.Join("/", new[] { cost1, cost2 }.Where(x => !string.IsNullOrWhiteSpace(x)));
		string set = wikiText.Match("\\|\\s*set = (.*)");
		string type = string.Join(" ", new[] { wikiText.Match("\\|\\s*type1 = (.*)"), wikiText.Match("\\|\\s*type2 = (.*)"), wikiText.Match("\\|\\s*type3 = (.*)") }.Where(x => !string.IsNullOrWhiteSpace(x)));
		string text = string.Join("\\n_____\\n", new[] { wikiText.Match("\\|\\s*text = (.*)"), wikiText.Match("\\|\\s*text2 = (.*)") }.Where(x => !string.IsNullOrWhiteSpace(x)));
		text = Regex.Replace(text, "<br/?>", "\\n");
		text = Regex.Replace(text, @"\{\{VP\}\}", "{VP}");
		text = Regex.Replace(text, @"\{\{VP\|'''(.)'''\|.\}\}", "$1 {VP}");
		text = Regex.Replace(text, @"\{\{Cost\|(\d+)(\|l)?\}\}", @"$$$1");
		text = Regex.Replace(text, @"\{\{Debt\|(\d+)(\|l)?\}\}", @"$1D");
		
		if(text.Contains("|")) text.Dump(name);
		
		$"{name}|{set}|{type}|{cost}|{text}".Dump();
		continue;
		
		//HtmlNode root = (await c.GetDocumentAsync($"http://wiki.dominionstrategy.com/index.php/{card}")).DocumentNode;
		//HtmlNode info = root.SelectSingleNode("//div[@id='mw-content-text']/table[1]");
		//string name = info.SelectSingleNode("tr[1]/th").InnerText.Dump("Name");
		//string cost = info.SelectSingleNode("tr[4]/td").InnerHtml.Dump("Cost");
	}
	
	"Done".Dump();
}

public static class StringExtensions
{
	public static string Match(this string input, string pattern, int group = 1)
	{
		Match m = Regex.Match(input, pattern);
		return m.Groups[group].Value;
	}
}

public static class HttpClientExtensions
{
	public static async Task<HtmlDocument> GetHtmlDocumentAsync(this HttpClient client, string uri)
	{
		string content = await client.GetStringAsync(uri);
		HtmlDocument d = new HtmlDocument();
		d.LoadHtml(content);
		return d;
	}
	
	public static async Task<XDocument> GetXDocumentAsync(this HttpClient client, string uri)
	{
		string content = await client.GetStringAsync(uri);
		return XDocument.Parse(content);
	}
}

// Define other methods and classes here