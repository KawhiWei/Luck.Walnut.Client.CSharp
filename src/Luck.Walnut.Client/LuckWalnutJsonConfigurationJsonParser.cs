using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Text.Json;

namespace Luck.Walnut.Client
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class LuckWalnutJsonConfigurationJsonParser
    {
        private LuckWalnutJsonConfigurationJsonParser()
        {
            _data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        }

        private readonly IDictionary<string, string> _data = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Stack<string> _context = new Stack<string>();
        private string _currentPath;
        public static IDictionary<string, string?> Parse(IDictionary<string, IDictionary<string, string>> pairs, string appId)
            => new LuckWalnutJsonConfigurationJsonParser().JsonParse(pairs, appId);

        private IDictionary<string, string?> JsonParse(IDictionary<string, IDictionary<string, string>> pairs, string appId)
        {

            var configDic = new Dictionary<string, string>();
            var configData = new Dictionary<string, string>();
            foreach (var pair in pairs)
            {
                foreach (var config in pair.Value)
                {
                    _data.Clear();
                    var jsonDocumentOptions = new JsonDocumentOptions
                    {
                        CommentHandling = JsonCommentHandling.Skip,
                        AllowTrailingCommas = true,
                    };
                    using (JsonDocument doc = JsonDocument.Parse(config.Value, jsonDocumentOptions))
                    {
                        VisitElement(doc.RootElement);
                    }
                    foreach (var data in _data)
                    {
                        configDic.Add(string.Join(":", config.Key, data.Key), data.Value);
                    }
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

            if (isEmpty && _context.Count > 0)
            {
                _data[_context.Peek()] = null;
            }
        }

        private void VisitValue(JsonElement value)
        {
            Debug.Assert(_context.Count > 0);

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
                    string key = _context.Peek();
                    if (_data.ContainsKey(key))
                    {
                        //throw new FormatException(SR.Format(SR.Error_KeyIsDuplicated, key));
                    }
                    _data[key] = value.ToString();
                    break;

                default:
                    throw new FormatException("");
            }
        }

        private void EnterContext(string context) =>
            _context.Push(_context.Count > 0 ?
                _context.Peek() + ConfigurationPath.KeyDelimiter + context :
                context);

        private void ExitContext() => _context.Pop();
    }
}
