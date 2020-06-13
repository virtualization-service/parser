using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Parser.Model
{
    public class MessageDto
    {
        public Uri service{ get; set;}

        public string operation{ get; set;}

        public Body request{ get; set;}

        public Body response{ get; set;}

        [JsonIgnore]
        public string service_component
        {
            get
            {
                return service?.AbsolutePath;
            }
        }

    }

    public class Body
    {
        public Dictionary<string,string[]> formatted_data{ get; set;}
        public Dictionary<string,string> headers{ get; set;}

        public string raw_data{ get; set;}

    }
}