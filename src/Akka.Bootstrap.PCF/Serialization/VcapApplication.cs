using System;
using System.Collections.Generic;
using System.Text;

namespace Akka.Bootstrap.PCF.Serialization
{
    public class Limits
    {
        public int disk { get; set; }
        public int fds { get; set; }
        public int mem { get; set; }
    }

    public class VcapApplication
    {
        public string application_id { get; set; }
        public string application_name { get; set; }
        public List<string> application_uris { get; set; }
        public string application_version { get; set; }
        public string cf_api { get; set; }
        public Limits limits { get; set; }
        public string name { get; set; }
        public string space_id { get; set; }
        public string space_name { get; set; }
        public List<string> uris { get; set; }
        public object users { get; set; }
        public string version { get; set; }
    }
}
