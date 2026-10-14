namespace Acme.Workforce.Workplace.Desk.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional desk mappings by code.</summary>
public interface IRegionalDeskMappingStore
{
    RegionalDeskMapping? Find(string code);
    void Save(RegionalDeskMapping item);
    bool Remove(string code);
    List<RegionalDeskMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalDeskMappingStore(Dictionary<string, RegionalDeskMapping> items, List<string> order) : IRegionalDeskMappingStore
{
    public Dictionary<string, RegionalDeskMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalDeskMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalDeskMapping item)
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

    public List<RegionalDeskMapping> List()
    {
        var outValue = new List<RegionalDeskMapping>();
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
public sealed class CachedRegionalDeskMappingStore(MemoryRegionalDeskMappingStore inner, RegionalDeskMappingCache cache, int hits, int misses) : IRegionalDeskMappingStore
{
    public MemoryRegionalDeskMappingStore Inner { get; set; } = inner;
    public RegionalDeskMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalDeskMapping? Find(string code)
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

    public void Save(RegionalDeskMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalDeskMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional desk mappings. Empty fields match all.</summary>
public sealed record RegionalDeskMappingQuery(string Country, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalDeskMapping item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
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

    public List<RegionalDeskMapping> Apply(List<RegionalDeskMapping> items)
    {
        var matched = Collections.Filter(items, (RegionalDeskMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional desk mapping store.</summary>
public static class RegionalDeskMappingStore
{
    public static IRegionalDeskMappingStore NewMemoryRegionalDeskMappingStore()
    {
        return new MemoryRegionalDeskMappingStore(new Dictionary<string, RegionalDeskMapping>(), new List<string>());
    }

    public static IRegionalDeskMappingStore NewCachedRegionalDeskMappingStore(MemoryRegionalDeskMappingStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalDeskMappingStore(inner, RegionalDeskMappingCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<RegionalDeskMapping> FindRegionalDeskMappings(IRegionalDeskMappingStore store, RegionalDeskMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
