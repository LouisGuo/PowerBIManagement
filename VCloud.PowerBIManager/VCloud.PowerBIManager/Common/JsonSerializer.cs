namespace VCloud.PowerBIManager
{
    #region using directives

    using System;
    using Newtonsoft.Json;
    using System.IO;
    #endregion using directives

    public class JsonSerializer
    {
        public static String ConvertObjToString<T>(T obj)
        {
            StringWriter textWriter = new StringWriter();
            JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
            {
                Formatting = Formatting.Indented,
                Indentation = 4,
                IndentChar = ' '
            };
            var serializer = new Newtonsoft.Json.JsonSerializer();
            serializer.Serialize(jsonWriter, obj);
            return textWriter.ToString();
        }

        public static T ConvertStringToObj<T>(String objString)
        {
            return JsonConvert.DeserializeObject<T>(objString);
        }
    }
}