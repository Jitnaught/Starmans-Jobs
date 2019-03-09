using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Starmans_Jobs.Classes
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Player
    {
        franklin = 0,
        trevor = 1,
        michael = 3,
        other = 4
    }
}
