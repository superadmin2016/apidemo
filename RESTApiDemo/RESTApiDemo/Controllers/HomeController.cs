using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RESTApiDemo.Models;
using System.Net.Http;
using System.Net.Http.Headers;

namespace RESTApiDemo.Controllers
{
  public class HomeController : Controller
  {
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
      _logger = logger;
    }

    public IActionResult Index()
    {
      return View();
    }

    public IActionResult Privacy()
    {
      return View();
    }

    public IActionResult MyTeam()
    {
      return View();
    }

    [Authorize(AuthenticationSchemes = "oauth")]
    public async Task<IActionResult> Transcript(string userId)
    {
      string accessToken = HttpContext.GetTokenAsync("access_token").Result ?? "No Data";
      var client = new HttpClient();
      client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
      string apiUrl = $"https://au11sales.sumtotaldevelopment.net/apis/api/v1/users/{ userId }/transcripts";
      var response = await client.GetAsync(apiUrl);
      ViewData["RawResponse"] = await response.Content.ReadAsStringAsync();
      return View("Transcripts", userId);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
      return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
  }
}
