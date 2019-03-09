 using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace Starmans_Jobs.Classes
{
    public class Job
    {
        // Define all variables that make up a job
        public string name { get; set; }
        public Degree degreeRequirement { get; set; }
        public Location location { get; set; }
        public List<EmployeePosition> positions { get; set; }
    }
}
