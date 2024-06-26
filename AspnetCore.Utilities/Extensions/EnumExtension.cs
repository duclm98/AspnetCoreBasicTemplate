using AspnetCore.Utilities.Models;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace AspnetCore.Utilities.Extensions;

public static class EnumExtension
{
    public static string GetDisplayName(this Enum enu)
    {
        if (enu == null)
            return null;
        var attr = GetDisplayAttribute(enu);
        return attr != null ? attr.Name : enu.ToString();
    }

    public static string GetDescription(this Enum enu)
    {
        var attr = GetDisplayAttribute(enu);
        return attr != null ? attr.Description : enu.ToString();
    }

    public static EnumValue GetValue(this Enum enu)
    {
        if (enu == null)
            return null;
        var attr = GetDisplayAttribute(enu);
        return new EnumValue
        {
            Value = Convert.ToInt32(enu),
            Name = enu.ToString(),
            DisplayName = attr != null ? attr.Name : enu.ToString()
        };
    }

    public static EnumValue GetValue<T>(int value)
    {
        foreach (var itemType in Enum.GetValues(typeof(T)))
            if ((int)itemType == value)
            {
                var type = itemType.GetType();
                var field = type.GetField(itemType.ToString() ?? string.Empty);

                return new EnumValue
                {
                    Value = (int)itemType,
                    Name = Enum.GetName(typeof(T), itemType),
                    DisplayName = field != null ? field.GetCustomAttribute<DisplayAttribute>().Name : null
                };
            }

        return null;
    }

    public static List<EnumValue> GetValues<T>()
    {
        var values = new List<EnumValue>();
        foreach (var itemType in Enum.GetValues(typeof(T)))
        {
            var type = itemType.GetType();
            var field = type.GetField(itemType.ToString() ?? string.Empty);
            values.Add(new EnumValue
            {
                Value = (int)itemType,
                Name = Enum.GetName(typeof(T), itemType),
                DisplayName = field != null ? field.GetCustomAttribute<DisplayAttribute>()?.Name : null
            });
        }

        return values;
    }

    public static List<int> GetIntValues<T>()
    {
        var results = new List<int>();
        foreach (var itemType in Enum.GetValues(typeof(T))) results.Add((int)itemType);
        return results;
    }

    private static DisplayAttribute GetDisplayAttribute(object value)
    {
        if (value == null)
            return null;
        var type = value.GetType();
        if (!type.IsEnum) throw new ArgumentException(string.Format("Type {0} is not an enum", type));

        // Get the enum field.
        var field = type.GetField(value.ToString() ?? string.Empty);
        return field == null ? null : field.GetCustomAttribute<DisplayAttribute>();
    }
}