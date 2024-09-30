using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Paralax.WebApi.Utils
{
    public static class Extensions
    {
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
            if (propertyInfo.CanWrite)
            {
                propertyInfo.SetValue(instance, value);
                return;
            }

            var fieldInfo = instance.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .SingleOrDefault(x => x.Name.StartsWith($"<{propertyInfo.Name}>"));

            fieldInfo?.SetValue(instance, value);
        }

        // Converts a string array to the required Dictionary type if possible
        public static bool TryConvertToDictionary(Type dictionaryType, object arrayValue, out object dictionary)
        {
            dictionary = null;
            if (!dictionaryType.IsGenericType || dictionaryType.GetGenericTypeDefinition() != typeof(Dictionary<,>))
            {
                return false;
            }

            var keyType = dictionaryType.GetGenericArguments()[0];
            var valueType = dictionaryType.GetGenericArguments()[1];

            if (keyType != typeof(string) || valueType != typeof(string))
            {
                return false;
            }

            var stringArray = arrayValue as string[];
            if (stringArray == null)
            {
                return false;
            }

            var dictionaryInstance = Activator.CreateInstance(dictionaryType) as IDictionary<string, string>;
            foreach (var item in stringArray)
            {
                dictionaryInstance[item] = item; // Simplified, use custom logic as needed
            }

            dictionary = dictionaryInstance;
            return true;
        }

        // Conversion handler for complex cases like string arrays to dictionaries
        public static object ConvertToExpectedType(Type targetType, object value)
        {
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                if (TryConvertToDictionary(targetType, value, out var dictionary))
                {
                    return dictionary;
                }
            }

            // Direct assignment fallback if no conversion is needed
            return value;
        }
    }
}
