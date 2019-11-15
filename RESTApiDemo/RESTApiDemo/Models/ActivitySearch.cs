using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RESTApiDemo.Models
{
  public class ActivitySearchResponse
  {
    public Pagination pagination { get; set; }

    public ActivitySearchResults[] data { get; set; }
  }

  public class ActivitySearchResults
  {
    public string activityId { get; set; }

    public string name { get; set; }
    public string description { get; set; }
    public string activityType { get; set; }

    public string activityImageUrl { get; set; }

    public string getActivityImageUrl(string baseUrl, string tenantKey)
        {
            if (activityImageUrl.StartsWith("data"))
            {
                return baseUrl + "/learning/DataStore/" + tenantKey + "/Learning/" + activityImageUrl;
            }
            return activityImageUrl;
        }
  }
}
