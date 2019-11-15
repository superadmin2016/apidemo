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
using Microsoft.Extensions.Caching.Memory;

namespace RESTApiDemo.Controllers
{
  public class HomeController : Controller
  {
    private readonly ILogger<HomeController> _logger;
    private readonly SumTotalSettings _sumtSettings;
    private readonly IMemoryCache _memoryCache;
    public HomeController(ILogger<HomeController> logger, IOptions<SumTotalSettings> sumtSettings, IMemoryCache memoryCache)
    {
      _logger = logger;
      _sumtSettings = sumtSettings.Value;
      _memoryCache = memoryCache;
    }

    [Authorize(AuthenticationSchemes = "oauth")]
    public async Task<IActionResult> Index()
    {
      //Get the count of employees using pagination on B2B APIs.
      var client = new HttpClient();
      client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetSumTotalAPIB2BAccessToken());
      string apiUrl = $"{ _sumtSettings.BaseUrl }/apis/api/v1/users?limit=1";
      var response = await client.GetAsync(apiUrl);

      JsonDocument jsonDoc = JsonDocument.Parse(response.Content.ReadAsStringAsync().Result);
      ViewData["EmployeeCount"] = jsonDoc.RootElement.GetProperty("pagination").GetProperty("total").GetInt32();

      //Get the count of activities using pagination of user context APIs.
      string accessToken = HttpContext.GetTokenAsync("access_token").Result ?? "No Data";
      client = new HttpClient();
      client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
      apiUrl = $"{ _sumtSettings.BaseUrl }/apis/api/v1/activities/search?limit=1";
      response = await client.GetAsync(apiUrl);
      if (!response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
      {
        await HttpContext.SignOutAsync();
        return this.RedirectToAction("Index", "Home");
      }
      jsonDoc = JsonDocument.Parse(response.Content.ReadAsStringAsync().Result);
      ViewData["ActivityCount"] = jsonDoc.RootElement.GetProperty("pagination").GetProperty("total").GetInt32();

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

    [Authorize(AuthenticationSchemes = "oauth")]
    public async Task<IActionResult> Search(string searchText)
    {
      string accessToken = HttpContext.GetTokenAsync("access_token").Result ?? "No Data";
      var client = new HttpClient();
      client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
      string apiUrl = $"{ _sumtSettings.BaseUrl }/apis/api/v1/activities/search?searchTerm={ searchText }";
      var response = await client.GetAsync(apiUrl);
      var responseJsonString = await response.Content.ReadAsStringAsync();
      ViewData["RawResponse"] = responseJsonString;

      if (!response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
      {
        await HttpContext.SignOutAsync();
        return this.RedirectToAction("Search", "Home", searchText);
      }
      else if(!response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.NotFound)
      {
        return View("Search", new ActivitySearchResponse() { pagination = new Pagination() { offset=0, limit=0, total=0 }, data= new List<ActivitySearchResults>().ToArray() });
      }

      var responseModel = JsonSerializer.Deserialize<ActivitySearchResponse>(responseJsonString);
      return View("Search", responseModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
      return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private string GetSumTotalAPIB2BAccessToken()
    {

      string accessToken;
      if (!_memoryCache.TryGetValue("SUMTAPIB2BACCESSTOKEN", out accessToken))
      {
        var content = GetPayLoadForClientCredentials(_sumtSettings);

        HttpClient httpClient = new HttpClient();
        var rawResponse = httpClient.PostAsync($"{ _sumtSettings.BaseUrl }/apisecurity/connect/token", content).Result;
        JsonDocument jsonDoc = JsonDocument.Parse(rawResponse.Content.ReadAsStringAsync().Result);
        accessToken = jsonDoc.RootElement.GetProperty("access_token").GetString();

        MemoryCacheEntryOptions cacheExpirationOptions = new MemoryCacheEntryOptions();
        cacheExpirationOptions.AbsoluteExpiration = DateTime.Now.AddMinutes(110);
        _memoryCache.Set("SUMTAPIB2BACCESSTOKEN", accessToken, cacheExpirationOptions);
      }
      
      return accessToken;
    }

    private static HttpContent GetPayLoadForClientCredentials(SumTotalSettings settings)
    {
      var scope = "allapis";

      var content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
      {
        new KeyValuePair<string, string>("client_id", settings.ClientId_CC),
        new KeyValuePair<string, string>("client_secret", settings.ClientSecret_CC),
        new KeyValuePair<string, string>("scope", scope),
        new KeyValuePair<string, string>("grant_type", "client_credentials"),
      });

      return content;
    }
  }
}
