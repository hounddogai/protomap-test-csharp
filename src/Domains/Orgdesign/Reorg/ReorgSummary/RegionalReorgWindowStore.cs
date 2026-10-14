namespace Acme.Workforce.Orgdesign.Reorg.ReorgSummary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional reorg windows by code.</summary>
public interface IRegionalReorgWindowStore
{
    RegionalReorgWindow? Find(string code);
    void Save(RegionalReorgWindow item);
    bool Remove(string code);
    List<RegionalReorgWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalReorgWindowStore(Dictionary<string, RegionalReorgWindow> items, List<string> order) : IRegionalReorgWindowStore
{
    public Dictionary<string, RegionalReorgWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalReorgWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalReorgWindow item)
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

    public List<RegionalReorgWindow> List()
    {
        var outValue = new List<RegionalReorgWindow>();
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
public sealed class CachedRegionalReorgWindowStore(MemoryRegionalReorgWindowStore inner, RegionalReorgWindowCache cache, int hits, int misses) : IRegionalReorgWindowStore
{
    public MemoryRegionalReorgWindowStore Inner { get; set; } = inner;
    public RegionalReorgWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalReorgWindow? Find(string code)
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

    public void Save(RegionalReorgWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalReorgWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional reorg windows. Empty fields match all.</summary>
public sealed record RegionalReorgWindowQuery(string Category, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalReorgWindow item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalReorgWindow> Apply(List<RegionalReorgWindow> items)
    {
        var matched = Collections.Filter(items, (RegionalReorgWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional reorg window store.</summary>
public static class RegionalReorgWindowStore
{
    public static IRegionalReorgWindowStore NewMemoryRegionalReorgWindowStore()
    {
        return new MemoryRegionalReorgWindowStore(new Dictionary<string, RegionalReorgWindow>(), new List<string>());
    }

    public static IRegionalReorgWindowStore NewCachedRegionalReorgWindowStore(MemoryRegionalReorgWindowStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalReorgWindowStore(inner, RegionalReorgWindowCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<RegionalReorgWindow> FindRegionalReorgWindows(IRegionalReorgWindowStore store, RegionalReorgWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
