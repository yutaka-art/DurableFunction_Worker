using Newtonsoft.Json;

namespace DurableFunction_Worker.Models
{
    public class ReceiveModel
    {
        [JsonProperty("TargetValue")]
        public int TargetValue { get; set; } = 0;
    }

}
