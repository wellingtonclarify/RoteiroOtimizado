using Newtonsoft.Json;
using System.Net.Http;

namespace RoteiroOtimizado
{
    public static class ExtensionsMethods
    {
        public static T ConvertTo<T>(this HttpResponseMessage message)
        {
            return ConvertTo<T>(message.Content);
        }

        public static T ConvertTo<T>(this HttpContent content)
        {
            var json = content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static void SaveJsonInFile(this object obj)
        {
            var json = JsonConvert.SerializeObject(obj);

            var tmpPath = System.IO.Path.GetTempFileName();
            var writer = new System.IO.StreamWriter(tmpPath);
            writer.Write(json);
            writer.Close();
            var path = System.IO.Path.ChangeExtension(tmpPath, "json");
            System.IO.File.Move(tmpPath, path);
            System.Diagnostics.Process.Start(path);
        }
    }
}
