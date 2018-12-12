using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Volcano.Interface
{
    class JsonColorConverter : JsonConverter
    {
        public override bool CanConvert(Type type)
        {
            return type == typeof(Color);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Color color = (Color)value;
            writer.WriteValue("#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2"));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return ColorTranslator.FromHtml(reader.Value.ToString());
        }
    }
}
