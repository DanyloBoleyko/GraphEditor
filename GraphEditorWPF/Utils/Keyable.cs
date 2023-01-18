using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphEditorWPF.Utils
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Keyable
    {
        protected string _key;

        public Keyable()
        {
            Key = GenerateKey();
        }

        [JsonProperty("key")]
        public string Key
        {
            get { return _key; }
            set 
            { 
                if (value != null)
                    _key = value;
            }
        }

        public string GenerateKey()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
