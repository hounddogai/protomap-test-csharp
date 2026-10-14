namespace Acme.Workforce.Engagement.TownHall.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional town hall bundles by code.</summary>
public interface IRegionalTownHallBundleStore
{
    RegionalTownHallBundle? Find(string code);
    void Save(RegionalTownHallBundle item);
    bool Remove(string code);
    List<RegionalTownHallBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalTownHallBundleStore(Dictionary<string, RegionalTownHallBundle> items, List<string> order) : IRegionalTownHallBundleStore
{
    public Dictionary<string, RegionalTownHallBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalTownHallBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalTownHallBundle item)
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

    public List<RegionalTownHallBundle> List()
    {
        var outValue = new List<RegionalTownHallBundle>();
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
public sealed class CachedRegionalTownHallBundleStore(MemoryRegionalTownHallBundleStore inner, RegionalTownHallBundleCache cache, int hits, int misses) : IRegionalTownHallBundleStore
{
    public MemoryRegionalTownHallBundleStore Inner { get; set; } = inner;
    public RegionalTownHallBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalTownHallBundle? Find(string code)
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

    public void Save(RegionalTownHallBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalTownHallBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional town hall bundles. Empty fields match all.</summary>
public sealed record RegionalTownHallBundleQuery(string Title, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalTownHallBundle item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalTownHallBundle> Apply(List<RegionalTownHallBundle> items)
    {
        var matched = Collections.Filter(items, (RegionalTownHallBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional town hall bundle store.</summary>
public static class RegionalTownHallBundleStore
{
    public static IRegionalTownHallBundleStore NewMemoryRegionalTownHallBundleStore()
    {
        return new MemoryRegionalTownHallBundleStore(
            new Dictionary<string, RegionalTownHallBundle>(),
            new List<string>());
    }

    public static IRegionalTownHallBundleStore NewCachedRegionalTownHallBundleStore(MemoryRegionalTownHallBundleStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalTownHallBundleStore(
            inner,
            RegionalTownHallBundleCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<RegionalTownHallBundle> FindRegionalTownHallBundles(IRegionalTownHallBundleStore store, RegionalTownHallBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
