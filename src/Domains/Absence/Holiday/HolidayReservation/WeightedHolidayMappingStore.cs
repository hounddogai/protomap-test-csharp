namespace Acme.Workforce.Absence.Holiday.HolidayReservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted holiday mappings by code.</summary>
public interface IWeightedHolidayMappingStore
{
    WeightedHolidayMapping? Find(string code);
    void Save(WeightedHolidayMapping item);
    bool Remove(string code);
    List<WeightedHolidayMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedHolidayMappingStore(Dictionary<string, WeightedHolidayMapping> items, List<string> order) : IWeightedHolidayMappingStore
{
    public Dictionary<string, WeightedHolidayMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedHolidayMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedHolidayMapping item)
    {
        if (!Items.ContainsKey(item.Code))
        {
            Order.Add(item.Code);
        }
        Items[item.Code] = item;
    }

    public bool Remove(string code)
    {
        if (!Items.ContainsKey(code))
        {
            return false;
        }
        Items.Remove(code);
        var kept = new List<string>();
        foreach (var existing in Order)
        {
            if (existing != code)
            {
                kept.Add(existing);
            }
        }
        Order = kept;
        return true;
    }

    public List<WeightedHolidayMapping> List()
    {
        var outValue = new List<WeightedHolidayMapping>();
        foreach (var key in Order)
        {
            if (!Items.TryGetValue(key, out var found))
            {
                continue;
            }
            outValue.Add(found);
        }
        return outValue;
    }
}

/// <summary>Serves reads from a short-lived cache in front of another store.</summary>
public sealed class CachedWeightedHolidayMappingStore(MemoryWeightedHolidayMappingStore inner, WeightedHolidayMappingCache cache, int hits, int misses) : IWeightedHolidayMappingStore
{
    public MemoryWeightedHolidayMappingStore Inner { get; set; } = inner;
    public WeightedHolidayMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedHolidayMapping? Find(string code)
    {
        if (Cache.Get(code) is not { } hit)
        {
            Misses += 1;
            if (Inner.Find(code) is not { } found)
            {
                return null;
            }
            Cache.Put(code, found);
            return found;
        }
        Hits += 1;
        return hit;
    }

    public void Save(WeightedHolidayMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedHolidayMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted holiday mappings. Empty fields match all.</summary>
public sealed record WeightedHolidayMappingQuery(string Name, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedHolidayMapping item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedHolidayMapping> Apply(List<WeightedHolidayMapping> items)
    {
        var matched = Collections.Filter(items, (WeightedHolidayMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted holiday mapping store.</summary>
public static class WeightedHolidayMappingStore
{
    public static IWeightedHolidayMappingStore NewMemoryWeightedHolidayMappingStore()
    {
        return new MemoryWeightedHolidayMappingStore(
            new Dictionary<string, WeightedHolidayMapping>(),
            new List<string>());
    }

    public static IWeightedHolidayMappingStore NewCachedWeightedHolidayMappingStore(MemoryWeightedHolidayMappingStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedHolidayMappingStore(
            inner,
            WeightedHolidayMappingCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<WeightedHolidayMapping> FindWeightedHolidayMappings(IWeightedHolidayMappingStore store, WeightedHolidayMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
