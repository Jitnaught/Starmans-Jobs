using System.Collections.Generic;
using GTA;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Starmans_Jobs.Classes
{
    public class PlayerData
    {
        // Define all variables that make up Player Data

        public Player player { get; set; }
        public Job job { get; set; }
        public List<Degree> degrees { get; set; }
    }
}
