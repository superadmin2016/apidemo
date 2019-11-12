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
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace RESTApiDemo.Controllers
{
  public class HomeController : Controller
  {
    private readonly ILogger<HomeController> _logger;
    private readonly SumTotalSettings _sumtSettings;

    public HomeController(ILogger<HomeController> logger, IOptions<SumTotalSettings> sumtSettings)
    {
      _logger = logger;
      _sumtSettings = sumtSettings.Value;
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
    public async Task<IActionResult> Transcript(string userId, string fullname)
    {
      string accessToken = HttpContext.GetTokenAsync("access_token").Result ?? "No Data";
      var client = new HttpClient();
      client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
      string apiUrl = $"{ _sumtSettings.BaseUrl }/apis/api/v1/users/{ userId }/transcripts";
      var response = await client.GetAsync(apiUrl);

      if (!response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
      {
        await HttpContext.SignOutAsync();
        return this.RedirectToAction("Transcript","Home", userId);
      }

      var responseJsonString = await response.Content.ReadAsStringAsync();
      var responseModel = JsonSerializer.Deserialize<UserTranscriptResponse>(responseJsonString);

      ViewData["RawResponse"] = responseJsonString;
      ViewData["UserId"] = userId;
      ViewData["FullName"] = fullname;
      return View("Transcripts", responseModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
      return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
  }
}
