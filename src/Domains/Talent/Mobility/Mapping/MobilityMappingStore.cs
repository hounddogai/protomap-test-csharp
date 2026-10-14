namespace Acme.Workforce.Talent.Mobility.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps mobility mappings by code.</summary>
public interface IMobilityMappingStore
{
    MobilityMapping? Find(string code);
    void Save(MobilityMapping item);
    bool Remove(string code);
    List<MobilityMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMobilityMappingStore(Dictionary<string, MobilityMapping> items, List<string> order) : IMobilityMappingStore
{
    public Dictionary<string, MobilityMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MobilityMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MobilityMapping item)
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

    public List<MobilityMapping> List()
    {
        var outValue = new List<MobilityMapping>();
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
public sealed class CachedMobilityMappingStore(MemoryMobilityMappingStore inner, MobilityMappingCache cache, int hits, int misses) : IMobilityMappingStore
{
    public MemoryMobilityMappingStore Inner { get; set; } = inner;
    public MobilityMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MobilityMapping? Find(string code)
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

    public void Save(MobilityMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MobilityMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects mobility mappings. Empty fields match all.</summary>
public sealed record MobilityMappingQuery(string Title, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(MobilityMapping item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<MobilityMapping> Apply(List<MobilityMapping> items)
    {
        var matched = Collections.Filter(items, (MobilityMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of mobility mapping store.</summary>
public static class MobilityMappingStore
{
    public static IMobilityMappingStore NewMemoryMobilityMappingStore()
    {
        return new MemoryMobilityMappingStore(new Dictionary<string, MobilityMapping>(), new List<string>());
    }

    public static IMobilityMappingStore NewCachedMobilityMappingStore(MemoryMobilityMappingStore inner, int ttl, IClock clock)
    {
        return new CachedMobilityMappingStore(inner, MobilityMappingCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<MobilityMapping> FindMobilityMappings(IMobilityMappingStore store, MobilityMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
