using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DurableFunction_Worker.Models
{
    public class ReturnModel
    {
        [JsonProperty("IsSucceed")]
        public bool IsSucceed { get; set; } = false;

        [JsonProperty("ProceedTime")]
        public double ProceedTime { get; set; } = 0;

        [JsonProperty("Exception")]
        public string Exception { get; set; } = "-";
    }
}
