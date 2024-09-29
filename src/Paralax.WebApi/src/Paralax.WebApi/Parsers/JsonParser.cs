using System;
using System.Collections.Generic;
using System.IO;
using NetJSON;

namespace Paralax.WebApi.Parsers
{
    // This JSON parser uses the NetJSON library for serialization/deserialization.
    // It parses a JSON string into key-value pairs suitable for configuration or other dictionary-based representations.
    public class JsonParser
    {
        private readonly Dictionary<string, string> _data = new(StringComparer.OrdinalIgnoreCase);

        public IDictionary<string, string> Parse(string json)
        {
            try
            {
                // Deserialize the JSON into a Dictionary<string, object>
                var deserializedData = NetJSON.NetJSON.Deserialize<Dictionary<string, object>>(json);
                ProcessDictionary(deserializedData, string.Empty);
            }
            catch (Exception ex)
            {
                throw new FormatException($"Failed to parse JSON: {ex.Message}", ex);
            }

            return _data;
        }

        private void ProcessDictionary(Dictionary<string, object> dict, string parentKey)
        {
            foreach (var kvp in dict)
            {
                var currentKey = string.IsNullOrEmpty(parentKey) ? kvp.Key : $"{parentKey}.{kvp.Key}";

                if (kvp.Value is Dictionary<string, object> nestedDict)
                {
                    // Recursively process nested dictionaries
                    ProcessDictionary(nestedDict, currentKey);
                }
                else if (kvp.Value is IList<object> list)
                {
                    // Process lists
                    ProcessList(list, currentKey);
                }
                else
                {
                    // Add to the data dictionary
                    if (_data.ContainsKey(currentKey))
                    {
                        throw new FormatException($"Duplicated key: {currentKey}");
                    }
                    _data[currentKey] = kvp.Value?.ToString(); 
                }
            }
        }

        private void ProcessList(IList<object> list, string parentKey)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var currentKey = $"{parentKey}[{i}]";

                if (list[i] is Dictionary<string, object> nestedDict)
                {
                    ProcessDictionary(nestedDict, currentKey);
                }
                else
                {
                    if (_data.ContainsKey(currentKey))
                    {
                        throw new FormatException($"Duplicated key: {currentKey}");
                    }
                    _data[currentKey] = list[i]?.ToString(); 
                }
            }
        }
    }
}
