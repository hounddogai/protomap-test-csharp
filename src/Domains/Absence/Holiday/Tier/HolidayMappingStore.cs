namespace Acme.Workforce.Absence.Holiday.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps holiday mappings by code.</summary>
public interface IHolidayMappingStore
{
    HolidayMapping? Find(string code);
    void Save(HolidayMapping item);
    bool Remove(string code);
    List<HolidayMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHolidayMappingStore(Dictionary<string, HolidayMapping> items, List<string> order) : IHolidayMappingStore
{
    public Dictionary<string, HolidayMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HolidayMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HolidayMapping item)
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

    public List<HolidayMapping> List()
    {
        var outValue = new List<HolidayMapping>();
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
public sealed class CachedHolidayMappingStore(MemoryHolidayMappingStore inner, HolidayMappingCache cache, int hits, int misses) : IHolidayMappingStore
{
    public MemoryHolidayMappingStore Inner { get; set; } = inner;
    public HolidayMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HolidayMapping? Find(string code)
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

    public void Save(HolidayMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HolidayMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects holiday mappings. Empty fields match all.</summary>
public sealed record HolidayMappingQuery(string Department, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(HolidayMapping item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HolidayMapping> Apply(List<HolidayMapping> items)
    {
        var matched = Collections.Filter(items, (HolidayMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of holiday mapping store.</summary>
public static class HolidayMappingStore
{
    public static IHolidayMappingStore NewMemoryHolidayMappingStore()
    {
        return new MemoryHolidayMappingStore(new Dictionary<string, HolidayMapping>(), new List<string>());
    }

    public static IHolidayMappingStore NewCachedHolidayMappingStore(MemoryHolidayMappingStore inner, int ttl, IClock clock)
    {
        return new CachedHolidayMappingStore(inner, HolidayMappingCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<HolidayMapping> FindHolidayMappings(IHolidayMappingStore store, HolidayMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
