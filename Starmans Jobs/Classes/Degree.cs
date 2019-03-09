using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Starmans_Jobs.Classes
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Degree
    {
        technology,
        law,
        pilot,
        acting,
        medical,
        engineering,
        none
    }
}
