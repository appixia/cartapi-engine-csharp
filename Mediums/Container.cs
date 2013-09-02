using System;
using System.Collections.Generic;

namespace Appixia.Engine.Mediums
{
    public class Container : Dictionary<string,object>
    {
        // TryGetValue replacement that returns null if not found
        public object Get(string key)
        {
            object obj = null;
            TryGetValue(key, out obj);
            return obj;
        }

        public void Put(string key, object value)
        {
            this[key] = value;
        }
    }
}
