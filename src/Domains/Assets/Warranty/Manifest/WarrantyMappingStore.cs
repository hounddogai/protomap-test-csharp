namespace Acme.Workforce.Assets.Warranty.Manifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps warranty mappings by code.</summary>
public interface IWarrantyMappingStore
{
    WarrantyMapping? Find(string code);
    void Save(WarrantyMapping item);
    bool Remove(string code);
    List<WarrantyMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWarrantyMappingStore(Dictionary<string, WarrantyMapping> items, List<string> order) : IWarrantyMappingStore
{
    public Dictionary<string, WarrantyMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WarrantyMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WarrantyMapping item)
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

    public List<WarrantyMapping> List()
    {
        var outValue = new List<WarrantyMapping>();
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
public sealed class CachedWarrantyMappingStore(MemoryWarrantyMappingStore inner, WarrantyMappingCache cache, int hits, int misses) : IWarrantyMappingStore
{
    public MemoryWarrantyMappingStore Inner { get; set; } = inner;
    public WarrantyMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WarrantyMapping? Find(string code)
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

    public void Save(WarrantyMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WarrantyMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects warranty mappings. Empty fields match all.</summary>
public sealed record WarrantyMappingQuery(string Segment, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(WarrantyMapping item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WarrantyMapping> Apply(List<WarrantyMapping> items)
    {
        var matched = Collections.Filter(items, (WarrantyMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of warranty mapping store.</summary>
public static class WarrantyMappingStore
{
    public static IWarrantyMappingStore NewMemoryWarrantyMappingStore()
    {
        return new MemoryWarrantyMappingStore(new Dictionary<string, WarrantyMapping>(), new List<string>());
    }

    public static IWarrantyMappingStore NewCachedWarrantyMappingStore(MemoryWarrantyMappingStore inner, int ttl, IClock clock)
    {
        return new CachedWarrantyMappingStore(inner, WarrantyMappingCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<WarrantyMapping> FindWarrantyMappings(IWarrantyMappingStore store, WarrantyMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
