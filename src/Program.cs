using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Web;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Text;

var ASCIIART = @"
  █████▒▄▄▄█████▓ ██▓███  ▓█████▄  ██▓  ██████  ▄████▄   ██▓     ▒█████    ██████  █    ██  ██▀███  ▓█████ 
▓██   ▒ ▓  ██▒ ▓▒▓██░  ██▒▒██▀ ██▌▓██▒▒██    ▒ ▒██▀ ▀█  ▓██▒    ▒██▒  ██▒▒██    ▒  ██  ▓██▒▓██ ▒ ██▒▓█   ▀ 
▒████ ░ ▒ ▓██░ ▒░▓██░ ██▓▒░██   █▌▒██▒░ ▓██▄   ▒▓█    ▄ ▒██░    ▒██░  ██▒░ ▓██▄   ▓██  ▒██░▓██ ░▄█ ▒▒███   
░▓█▒  ░ ░ ▓██▓ ░ ▒██▄█▓▒ ▒░▓█▄   ▌░██░  ▒   ██▒▒▓▓▄ ▄██▒▒██░    ▒██   ██░  ▒   ██▒▓▓█  ░██░▒██▀▀█▄  ▒▓█  ▄ 
░▒█░      ▒██▒ ░ ▒██▒ ░  ░░▒████▓ ░██░▒██████▒▒▒ ▓███▀ ░░██████▒░ ████▓▒░▒██████▒▒▒▒█████▓ ░██▓ ▒██▒░▒████▒
 ▒ ░      ▒ ░░   ▒▓▒░ ░  ░ ▒▒▓  ▒ ░▓  ▒ ▒▓▒ ▒ ░░ ░▒ ▒  ░░ ▒░▓  ░░ ▒░▒░▒░ ▒ ▒▓▒ ▒ ░░▒▓▒ ▒ ▒ ░ ▒▓ ░▒▓░░░ ▒░ ░
 ░          ░    ░▒ ░      ░ ▒  ▒  ▒ ░░ ░▒  ░ ░  ░  ▒   ░ ░ ▒  ░  ░ ▒ ▒░ ░ ░▒  ░ ░░░▒░ ░ ░   ░▒ ░ ▒░ ░ ░  ░
 ░ ░      ░      ░░        ░ ░  ░  ▒ ░░  ░  ░  ░          ░ ░   ░ ░ ░ ▒  ░  ░  ░   ░░░ ░ ░   ░░   ░    ░   
                             ░     ░        ░  ░ ░          ░  ░    ░ ░        ░     ░        ░        ░  ░";

Console.ForegroundColor = ConsoleColor.Magenta;
Console.WriteLine(ASCIIART);
Console.ResetColor();

var options = new ChromeOptions();
options.AddArgument("--headless");

ChromeDriverService service = ChromeDriverService.CreateDefaultService();
service.HideCommandPromptWindow = true;

var dork = "intitle:\"index of\" inurl:ftp";
var pattern = @"https?://[^\s›]+";

using (var driver = new ChromeDriver(service, options))
{
    Console.WriteLine("[*] Extracting servers...");

    driver.Navigate().GoToUrl($"https://www.google.com/search?q={dork}&num=999999"); //lol
    Console.WriteLine(driver.Title);

    var links = driver.FindElements(By.TagName("cite"));

    foreach (var link in links)
    {

        var linkDecoded = HttpUtility.UrlDecode(link.Text).Trim();

        Match match = Regex.Match(linkDecoded, pattern);

        if (match.ToString().StartsWith("https://") || match.ToString().StartsWith("http://"))
        {
            var hostOnly = match.ToString().Replace("http://", "");
            hostOnly = match.ToString().Replace("https://", "");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(hostOnly);

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(await ConnectToFTP(hostOnly));
        }

        Console.ResetColor();
    }
}

async Task<string> ConnectToFTP(string host)
{
    using TcpClient client = new TcpClient();

    try
    {
        client.ConnectAsync(host, 21).Wait(5000);

        NetworkStream stream = client.GetStream();

        var data = new Byte[4096];
        var bytes = await stream.ReadAsync(data, 0, data.Length);
        var responseData = Encoding.ASCII.GetString(data, 0, bytes);

        return responseData;
    }
    catch
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Server offline, skipping...");
        return null;
    }
}
