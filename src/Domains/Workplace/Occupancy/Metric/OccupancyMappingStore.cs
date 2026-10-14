namespace Acme.Workforce.Workplace.Occupancy.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps occupancy mappings by code.</summary>
public interface IOccupancyMappingStore
{
    OccupancyMapping? Find(string code);
    void Save(OccupancyMapping item);
    bool Remove(string code);
    List<OccupancyMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOccupancyMappingStore(Dictionary<string, OccupancyMapping> items, List<string> order) : IOccupancyMappingStore
{
    public Dictionary<string, OccupancyMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OccupancyMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OccupancyMapping item)
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

    public List<OccupancyMapping> List()
    {
        var outValue = new List<OccupancyMapping>();
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
public sealed class CachedOccupancyMappingStore(MemoryOccupancyMappingStore inner, OccupancyMappingCache cache, int hits, int misses) : IOccupancyMappingStore
{
    public MemoryOccupancyMappingStore Inner { get; set; } = inner;
    public OccupancyMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OccupancyMapping? Find(string code)
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

    public void Save(OccupancyMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OccupancyMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects occupancy mappings. Empty fields match all.</summary>
public sealed record OccupancyMappingQuery(string Locale, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(OccupancyMapping item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<OccupancyMapping> Apply(List<OccupancyMapping> items)
    {
        var matched = Collections.Filter(items, (OccupancyMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of occupancy mapping store.</summary>
public static class OccupancyMappingStore
{
    public static IOccupancyMappingStore NewMemoryOccupancyMappingStore()
    {
        return new MemoryOccupancyMappingStore(new Dictionary<string, OccupancyMapping>(), new List<string>());
    }

    public static IOccupancyMappingStore NewCachedOccupancyMappingStore(MemoryOccupancyMappingStore inner, int ttl, IClock clock)
    {
        return new CachedOccupancyMappingStore(inner, OccupancyMappingCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<OccupancyMapping> FindOccupancyMappings(IOccupancyMappingStore store, OccupancyMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
