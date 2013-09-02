using System;
using System.Collections.Generic;
using Appixia.Engine.Mediums;

namespace Appixia.Engine
{
    public class Request
    {
        public Container Data { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
        public Encoder Encoder { get; set; }
    }
}
