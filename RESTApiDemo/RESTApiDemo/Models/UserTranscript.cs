using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RESTApiDemo.Models
{
  public class UserTranscriptResponse
  {
    public Pagination pagination { get; set; }

    public Activity[] data { get; set; }
  }

  

  public class Activity
  {
    public string activityName { get; set; }

    public decimal? estimatedCreditHours { get; set; }
    public DateTime starrtDate { get; set; }

    public DateTime completionDate { get; set; }

    public decimal? score { get; set; }

    public string grade { get; set; }
  }
}
