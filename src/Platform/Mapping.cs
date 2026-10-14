using System.Collections;
using Google.Protobuf;

namespace Acme.Workforce.Platform;

/// <summary>A domain command preserves the payload while routing information changes between workflow stages.</summary>
public sealed record DispatchCommand<T>(T Payload, string Region, string TenantId);

/// <summary>Explicit mapping profiles keep transport contracts out of application decisions.</summary>
public sealed class MappingProfile<TSource, TDestination>(Func<TSource, TDestination> conversion)
{
    public TDestination Map(TSource source) => conversion(source);
}

/// <summary>Copies public properties for legacy adapters whose field names are configured at runtime.</summary>
public static class PropertyMap
{
    public static T Copy<T>(T source) where T : class, new()
    {
        var destination = new T();
        var inactive = source is IMessage message
            ? message.Descriptor.Fields.InFieldNumberOrder()
                .Where(field => field.ContainingOneof is { } group && group.Accessor.GetCaseFieldDescriptor(message) != field)
                .Select(field => field.PropertyName).ToHashSet(StringComparer.Ordinal)
            : new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in typeof(T).GetProperties().OrderBy(property => property.Name, StringComparer.Ordinal))
        {
            if (!property.CanRead || property.GetIndexParameters().Length != 0 || property.GetMethod?.IsStatic == true)
                continue;
            if (inactive.Contains(property.Name))
                continue;
            var value = property.GetValue(source);
            if (value is null)
                continue;
            if (property.CanWrite)
                property.SetValue(destination, value);
            else if (value is IDictionary dictionary && property.GetValue(destination) is IDictionary targetMap)
                foreach (DictionaryEntry entry in dictionary)
                    targetMap.Add(entry.Key, entry.Value);
            else if (value is IList list && property.GetValue(destination) is IList targetList)
                foreach (var item in list)
                    targetList.Add(item);
        }
        return destination;
    }
}

/// <summary>Small transformations used by domain profiles when an upstream contract changes cardinality.</summary>
public static class Projection
{
    public static Dictionary<string, string> Labels(IEnumerable<KeyValuePair<string, string>> values) =>
        values.Select(entry => new { entry.Key, entry.Value })
            .ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal);

    public static IEnumerable<T> Flatten<T>(IEnumerable<T> values) =>
        values.Select(value => new[] { value }).SelectMany(batch => batch);
}
