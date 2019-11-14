using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RESTApiDemo.Models
{
  public class Pagination
  {
    public int offset { get; set; }
    public int limit { get; set; }
    public int total { get; set; }
  }
}
