using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace jf.Models
{
    public class ObjectToIdentify
    {
        public IFormFile Image2 { get; set; }
        public String GroupName { get; set; }
        public string ImageUrl { get; set; }
    }
}
