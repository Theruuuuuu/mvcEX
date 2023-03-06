using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Models
{
    public class Field
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class Location
    {
        [JsonProperty("locationName")]
        public string LocationName { get; set; }

        [JsonProperty("weatherElement")]
        public List<WeatherElement> WeatherElement { get; set; }
    }

    public class Parameter
    {
        [JsonProperty("parameterName")]
        public string ParameterName { get; set; }

        [JsonProperty("parameterValue")]
        public string ParameterValue { get; set; }

        [JsonProperty("parameterUnit")]
        public string ParameterUnit { get; set; }
    }

    public class Records
    {
        [JsonProperty("datasetDescription")]
        public string DatasetDescription { get; set; }

        [JsonProperty("location")]
        public List<Location> Location { get; set; }
    }

    public class Result
    {
        [JsonProperty("resource_id")]
        public string ResourceId { get; set; }

        [JsonProperty("fields")]
        public List<Field> Fields { get; set; }
    }

    public class Root
    {
        [JsonProperty("success")]
        public string Success { get; set; }

        [JsonProperty("result")]
        public Result Result { get; set; }

        [JsonProperty("records")]
        public Records Records { get; set; }
    }

    public class Time
    {
        [JsonProperty("startTime")]
        public string StartTime { get; set; }

        [JsonProperty("endTime")]
        public string EndTime { get; set; }

        [JsonProperty("parameter")]
        public Parameter Parameter { get; set; }
    }

    public class WeatherElement
    {
        [JsonProperty("elementName")]
        public string ElementName { get; set; }

        [JsonProperty("time")]
        public List<Time> Time { get; set; }
    }
}
