using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Luck.Walnut.Client
{
    internal sealed class LuckWalnutJsonConfigurationJsonParser
    {
        private LuckWalnutJsonConfigurationJsonParser() { }

        private readonly Dictionary<string, string?> _data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        private readonly Stack<string> _paths = new Stack<string>();

        public static IDictionary<string, string?> Parse(IDictionary<string, IDictionary<string, string>> pairs,string appId)
            => new LuckWalnutJsonConfigurationJsonParser().JsonParse(pairs, appId);

        private IDictionary<string, string?> JsonParse(IDictionary<string, IDictionary<string, string>> pairs, string appId)
        {

            var configDic = new Dictionary<string, string>();
            var configData = new Dictionary<string, string>();
            foreach (var pair in pairs)
            {
                foreach (var config in pair.Value)
                {
                    configDic.Add(config.Key, config.Value);
                }
                if (pair.Key == appId)
                {
                    foreach (var dic in configDic)
                    {
                        configData.Add(dic.Key, dic.Value);
                    }
                    configDic.Clear();

                }
            }


            //var jsonDocumentOptions = new JsonDocumentOptions
            //{
            //    CommentHandling = JsonCommentHandling.Skip,
            //    AllowTrailingCommas = true,
            //};
            //using (JsonDocument doc = JsonDocument.Parse(input, jsonDocumentOptions))
            //{
            //    if (doc.RootElement.ValueKind != JsonValueKind.Object)
            //    {

            //    }
            //    VisitElement(doc.RootElement);
            //}





            return configData;
        }

        private void VisitElement(JsonElement element)
        {
            var isEmpty = true;

            foreach (JsonProperty property in element.EnumerateObject())
            {
                isEmpty = false;
                EnterContext(property.Name);
                VisitValue(property.Value);
                ExitContext();
            }

            if (isEmpty && _paths.Count > 0)
            {
                _data[_paths.Peek()] = null;
            }
        }

        private void VisitValue(JsonElement value)
        {
            //Debug.Assert(_paths.Count > 0);

            switch (value.ValueKind)
            {
                case JsonValueKind.Object:
                    VisitElement(value);
                    break;

                case JsonValueKind.Array:
                    int index = 0;
                    foreach (JsonElement arrayElement in value.EnumerateArray())
                    {
                        EnterContext(index.ToString());
                        VisitValue(arrayElement);
                        ExitContext();
                        index++;
                    }
                    break;

                case JsonValueKind.Number:
                case JsonValueKind.String:
                case JsonValueKind.True:
                case JsonValueKind.False:
                case JsonValueKind.Null:
                    string key = _paths.Peek();
                    if (_data.ContainsKey(key))
                    {
                    }
                    _data[key] = value.ToString();
                    break;

                default:
                    throw new ArgumentNullException();
            }
        }

        private void EnterContext(string context) =>
            _paths.Push(_paths.Count > 0 ?
                _paths.Peek() + ConfigurationPath.KeyDelimiter + context :
                context);

        private void ExitContext() => _paths.Pop();


    }
}
