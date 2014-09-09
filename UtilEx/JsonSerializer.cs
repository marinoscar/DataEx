using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UtilEx
{
    public class JsonSerializer
    {
        public static string ToJson(object data)
        {
            return JsonConvert.SerializeObject(data);
        }

        public static T FromJson<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
