using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Paralax.WebApi.Parsers
{
    // JSON parser using System.Text.Json for serialization/deserialization.
    // It parses a JSON string into key-value pairs suitable for configuration or other dictionary-based representations.
    public class JsonParser
    {
        private readonly Dictionary<string, string> _data = new(StringComparer.OrdinalIgnoreCase);
        private readonly Stack<string> _stack = new();

        public IDictionary<string, string> Parse(string json)
        {
            var jsonDocumentOptions = new JsonDocumentOptions
            {
                CommentHandling = JsonCommentHandling.Skip,  // Skip comments in JSON
                AllowTrailingCommas = true                    // Allow trailing commas
            };

            // Parse the JSON string using System.Text.Json
            using (JsonDocument doc = JsonDocument.Parse(json, jsonDocumentOptions))
            {
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                {
                    throw new FormatException($"Invalid top-level JSON element: {doc.RootElement.ValueKind}");
                }

                // Traverse the JSON structure
                VisitElement(doc.RootElement);
            }

            return _data;
        }

        // Recursively visit each JSON element
        private void VisitElement(JsonElement element)
        {
            var isEmpty = true;

            // Process each property in the JSON object
            foreach (JsonProperty property in element.EnumerateObject())
            {
                isEmpty = false;
                EnterContext(property.Name);
                VisitValue(property.Value);
                ExitContext();
            }

            // Handle empty objects
            if (isEmpty && _stack.Count > 0)
            {
                _data[_stack.Peek()] = null;
            }
        }

        // Process each value in the JSON object or array
        private void VisitValue(JsonElement value)
        {
            switch (value.ValueKind)
            {
                case JsonValueKind.Object:
                    // If it's an object, visit its properties
                    VisitElement(value);
                    break;

                case JsonValueKind.Array:
                    // If it's an array, process each element
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
                    var key = _stack.Peek();

                    if (_data.ContainsKey(key))
                    {
                        throw new FormatException($"Duplicated key: {key}");
                    }

                    // Store the value as a string
                    _data[key] = value.ToString();
                    break;

                default:
                    throw new FormatException($"Unsupported JSON token: {value.ValueKind}");
            }
        }

        // Enter a new context in the JSON structure (i.e., navigate deeper)
        private void EnterContext(string context)
        {
            _stack.Push(_stack.Count > 0
                ? _stack.Peek() + "." + context  // Use '.' to concatenate nested keys
                : context);
        }

        // Exit the current context (i.e., navigate back up)
        private void ExitContext()
        {
            _stack.Pop();
        }
    }
}
