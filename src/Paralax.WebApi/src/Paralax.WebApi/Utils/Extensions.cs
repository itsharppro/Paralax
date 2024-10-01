using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Paralax.WebApi.Utils
{
    public static class Extensions
    {
        private static readonly Regex JsDateRegex = new Regex(@"\/Date\((\d+)(?:[+-]\d+)?\)\/", RegexOptions.Compiled);
        // Returns the default instance of the specified type
        public static object GetDefaultInstance(this Type type)
        {
            if (type == typeof(string))
            {
                return string.Empty;
            }

            var defaultValueCache = new Dictionary<Type, object>();

            if (TryGetDefaultValue(type, out var instance, defaultValueCache))
            {
                return instance;
            }

            return default;
        }

        // Sets default values for all properties of the given instance
        public static object SetDefaultInstanceProperties(this object instance)
            => SetDefaultInstanceProperties(instance, new Dictionary<Type, object>());

        // Helper method that sets default values for properties, utilizing a cache for efficiency
        private static object SetDefaultInstanceProperties(object instance, Dictionary<Type, object> defaultValueCache)
        {
            defaultValueCache ??= new Dictionary<Type, object>();

            var type = instance.GetType();

            foreach (var propertyInfo in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (TryGetDefaultValue(propertyInfo.PropertyType, out var defaultValue, defaultValueCache))
                {
                    SetValue(propertyInfo, instance, defaultValue);
                }
            }

            return instance;
        }

        // Tries to get the default value for a given type, using a cache to store previously calculated defaults
        private static bool TryGetDefaultValue(Type type, out object defaultValue, Dictionary<Type, object> defaultValueCache)
        {
            if (defaultValueCache.TryGetValue(type, out defaultValue))
            {
                return true;
            }

            if (type == typeof(string))
            {
                defaultValue = string.Empty;
                defaultValueCache[type] = defaultValue;
                return true;
            }

            // Handle IDictionary<K,V> types like IDictionary`
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDictionary<,>))
            {
                defaultValue = null; // ConvIDictionary` returns null for IDictionary
                return false;
            }

            // Handle Dictionary<K,V> types
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                var keyType = type.GetGenericArguments()[0];
                var valueType = type.GetGenericArguments()[1];
                var dictionaryType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
                defaultValue = Activator.CreateInstance(dictionaryType);
                defaultValueCache[type] = defaultValue;
                return true;
            }

            // Handle List<T> types
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                var elementType = type.GetGenericArguments()[0];
                var listType = typeof(List<>).MakeGenericType(elementType);
                defaultValue = Activator.CreateInstance(listType);
                defaultValueCache[type] = defaultValue;
                return true;
            }

            // Handle IEnumerable and arrays
            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                if (TryGetCollectionDefaultValue(type, out defaultValue))
                {
                    defaultValueCache[type] = defaultValue;
                    return true;
                }

                defaultValue = null;
                return false;
            }

            // Handle abstract class or interface
            if (type.IsInterface || type.IsAbstract)
            {
                defaultValue = null;
                return false;
            }

            // Handle arrays
            if (type.IsArray)
            {
                if (TryGetCollectionDefaultValue(type, out defaultValue))
                {
                    defaultValueCache[type] = defaultValue;
                    return true;
                }

                defaultValue = null;
                return false;
            }

            // Handle struct or class
            if (type.IsClass)
            {
                // Create an uninitialized instance of the class
                defaultValue = FormatterServices.GetUninitializedObject(type);
                defaultValueCache[type] = defaultValue;
                SetDefaultInstanceProperties(defaultValue, defaultValueCache);
                return true;
            }

            if (type.IsValueType)
            {
                // Create a default value for a struct
                defaultValue = Activator.CreateInstance(type);
                defaultValueCache[type] = defaultValue;
                return true;
            }

            defaultValue = null;
            return false;
        }

        // Attempts to get a default value for a collection type (e.g., array, List<T>, etc.)
        private static bool TryGetCollectionDefaultValue(Type type, out object defaultValue)
        {
            var elementType = type.IsGenericType ? type.GenericTypeArguments[0] : type.GetElementType();

            if (elementType is null)
            {
                defaultValue = null;
                return false;
            }

            if (elementType == typeof(string))
            {
                defaultValue = Array.Empty<string>();
                return true;
            }

            // Handle IEnumerable<T> or other collections
            if (typeof(IEnumerable).IsAssignableFrom(elementType))
            {
                defaultValue = Array.CreateInstance(elementType, 0);
                return true;
            }

            defaultValue = Array.CreateInstance(elementType, 0);
            return true;
        }

        // Sets the value of a property or field on an object
        private static void SetValue(PropertyInfo propertyInfo, object instance, object value)
        {
            var expectedType = propertyInfo.PropertyType;

            // Convert the value to the expected type
            var convertedValue = ConvertToExpectedType(expectedType, value);

            if (propertyInfo.CanWrite)
            {
                propertyInfo.SetValue(instance, convertedValue);
            }
            else
            {
                var fieldInfo = instance.GetType()
                    .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                    .SingleOrDefault(x => x.Name.StartsWith($"<{propertyInfo.Name}>"));

                fieldInfo?.SetValue(instance, convertedValue);
            }
        }

        // Converts a string array to the required Dictionary type if possible
        public static bool TryConvertToDictionary(Type dictionaryType, object arrayValue, out object dictionary)
        {
            dictionary = null;

            // Check if the target type is a generic Dictionary<string, string>
            if (!dictionaryType.IsGenericType || dictionaryType.GetGenericTypeDefinition() != typeof(Dictionary<,>))
            {
                return false;
            }

            var keyType = dictionaryType.GetGenericArguments()[0];
            var valueType = dictionaryType.GetGenericArguments()[1];

            // Ensure both key and value types are string
            if (keyType != typeof(string) || valueType != typeof(string))
            {
                return false;
            }

            // Convert string array to dictionary (use array index as key or custom logic)
            var stringArray = arrayValue as string[];
            if (stringArray == null)
            {
                return false;
            }

            // Create dictionary and map array elements to keys and values
            var dictionaryInstance = Activator.CreateInstance(dictionaryType) as IDictionary<string, string>;

            for (int i = 0; i < stringArray.Length; i++)
            {
                dictionaryInstance[$"Key_{i}"] = stringArray[i]; // Using "Key_" + index as the key, string as the value
            }

            dictionary = dictionaryInstance;
            return true;
        }

        public static object ConvertToExpectedType(Type targetType, object value)
        {
            // If the value is already of the target type, return it directly
            if (value != null && targetType.IsAssignableFrom(value.GetType()))
            {
                return value;
            }

            // Handle Guid types directly
            if (targetType == typeof(Guid) && value is Guid)
            {
                return value;
            }

            // Handle conversion from string[] to IEnumerable<string>
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                var elementType = targetType.GetGenericArguments()[0];
                if (value.GetType().IsArray && value.GetType().GetElementType() == elementType)
                {
                    return value;
                }

                if (typeof(IEnumerable).IsAssignableFrom(value.GetType()))
                {
                    var list = Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType)) as IList;
                    foreach (var item in (IEnumerable)value)
                    {
                        list.Add(Convert.ChangeType(item, elementType));
                    }
                    return list;
                }
            }

            // Check if the target type is a dictionary
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                if (TryConvertToDictionary(targetType, value, out var dictionary))
                {
                    return dictionary;
                }
            }

            // Date handling
            if (targetType == typeof(DateTime))
            {
                return ParseDateTime(value);
            }


            // Handle nullable types
            if (Nullable.GetUnderlyingType(targetType) != null)
            {
                if (value == null)
                    return null;

                targetType = Nullable.GetUnderlyingType(targetType);
            }

            // For value types and simple reference types, fallback to Convert.ChangeType
            if (value is IConvertible)
            {
                return Convert.ChangeType(value, targetType);
            }

            throw new InvalidCastException($"Cannot convert value of type {value.GetType()} to {targetType}");
        }

        // Extension to convert any object to another type by utilizing property-to-property mapping
        public static T ConvertTo<T>(this object value)
        {
            var targetType = typeof(T);

            // Handle simple types directly
            if (targetType.IsAssignableFrom(value.GetType()))
            {
                return (T)value;
            }

            // Handle more complex conversions
            var instance = Activator.CreateInstance<T>();
            var properties = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                var sourceProperty = value.GetType().GetProperty(property.Name);
                if (sourceProperty != null)
                {
                    var sourceValue = sourceProperty.GetValue(value);
                    var convertedValue = ConvertToExpectedType(property.PropertyType, sourceValue);
                    property.SetValue(instance, convertedValue);
                }
            }

            return instance;
        }

        public static DateTime? ParseDateTime(object value)
        {
            if (value == null) return null;

            // Check for JS-style date \/Date(...)
            var stringValue = value.ToString();
            var match = JsDateRegex.Match(stringValue);
            if (match.Success && long.TryParse(match.Groups[1].Value, out var unixTimeMs))
            {
                // Convert milliseconds since Unix epoch
                return DateTimeOffset.FromUnixTimeMilliseconds(unixTimeMs).UtcDateTime;
            }

            // Try ISO 8601 date parsing
            if (DateTime.TryParse(stringValue, null, System.Globalization.DateTimeStyles.RoundtripKind, out var parsedDate))
            {
                return parsedDate;
            }

            // Try parsing as Unix timestamp
            if (long.TryParse(stringValue, out var unixTimeSeconds))
            {
                return DateTimeOffset.FromUnixTimeSeconds(unixTimeSeconds).UtcDateTime;
            }

            return null;
        }
    }
}
