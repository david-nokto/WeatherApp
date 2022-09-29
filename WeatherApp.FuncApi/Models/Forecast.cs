using System;
using System.Collections.Generic;

namespace WeatherApp.FuncApi.Models
{
    public partial class Forecast
    {
        public DateTimeOffset ApprovedTime { get; set; }
        public DateTimeOffset ReferenceTime { get; set; }
        public Geography Geography { get; set; }
        public List<TimeSeries> TimeSeries { get; set; }
    }

    public partial class Geography
    {
        public string Type { get; set; }
        public List<List<double>> Coordinates { get; set; }
    }

    public partial class TimeSeries
    {
        public DateTimeOffset ValidTime { get; set; }
        public List<Parameter> Parameters { get; set; }
    }

    public partial class Parameter
    {
        public string Name { get; set; }
        public string LevelType { get; set; }
        public long Level { get; set; }
        public string Unit { get; set; }
        public List<double> Values { get; set; }
    }
}

