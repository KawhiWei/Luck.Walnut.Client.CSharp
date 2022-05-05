using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Text.Json;

namespace Luck.Walnut.Client
{
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
                    var jsonConfig = JObject.Parse(config.Value);
                    VisitJObject(jsonConfig);
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

        private void VisitJObject(JObject? jObject)
        {
            foreach (var property in jObject.Properties())
            {
                try
                {
                    EnterContext(property.Name);
                    VisitProperty(property);

                }
                finally
                {
                    ExitContext();
                }
            }
        }

        private void VisitProperty(JProperty property)
        {
            VisitToken(property.Value);
        }

        private void VisitToken(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    VisitJObject(token.Value<JObject>());
                    break;

                case JTokenType.Array:
                    VisitArray(token.Value<JArray>());
                    break;

                case JTokenType.Integer:
                case JTokenType.Float:
                case JTokenType.String:
                case JTokenType.Boolean:
                case JTokenType.Bytes:
                case JTokenType.Raw:
                case JTokenType.Null:
                case JTokenType.Date:
                    VisitPrimitive(token.Value<JValue>());
                    break;

                default:
                    //Log.Error($"不支持此{token}-{token.Type}类型的转换");
                    throw new FormatException($"不支持此{token}-{token.Type}类型的转换");
                    //throw new FormatException(Resources.FormatError_UnsupportedJSONToken(
                    //    _reader.TokenType,
                    //    _reader.Path,
                    //    _reader.LineNumber,
                    //    _reader.LinePosition));
            }
        }

        private void VisitArray(JArray? array)
        {
            for (int index = 0; index < array.Count; index++)
            {
                EnterContext(index.ToString());
                VisitToken(array[index]);
                ExitContext();
            }
        }

        private void VisitPrimitive(JValue data)
        {
            var key = _currentPath;

            if (_data.ContainsKey(key))
            {
                //throw new FormatException(Resources.FormatError_KeyIsDuplicated(key));
                //Log.Error($"key：{key}重复");
                throw new FormatException($"key：{key}重复");
            }
            _data[key] = data.ToString(CultureInfo.InvariantCulture);
        }

        private void EnterContext(string context)
        {
            _context.Push(context);
            _currentPath = ConfigurationPath.Combine(_context.Reverse());
        }

        private void ExitContext()
        {
            _context.Pop();
            _currentPath = ConfigurationPath.Combine(_context.Reverse());
        }


    }
}
