namespace Acme.Workforce.Orgdesign.Headcount.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps headcount bundles by code.</summary>
public interface IHeadcountBundleStore
{
    HeadcountBundle? Find(string code);
    void Save(HeadcountBundle item);
    bool Remove(string code);
    List<HeadcountBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHeadcountBundleStore(Dictionary<string, HeadcountBundle> items, List<string> order) : IHeadcountBundleStore
{
    public Dictionary<string, HeadcountBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HeadcountBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HeadcountBundle item)
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

    public List<HeadcountBundle> List()
    {
        var outValue = new List<HeadcountBundle>();
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
public sealed class CachedHeadcountBundleStore(MemoryHeadcountBundleStore inner, HeadcountBundleCache cache, int hits, int misses) : IHeadcountBundleStore
{
    public MemoryHeadcountBundleStore Inner { get; set; } = inner;
    public HeadcountBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HeadcountBundle? Find(string code)
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

    public void Save(HeadcountBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HeadcountBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects headcount bundles. Empty fields match all.</summary>
public sealed record HeadcountBundleQuery(string Label, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(HeadcountBundle item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HeadcountBundle> Apply(List<HeadcountBundle> items)
    {
        var matched = Collections.Filter(items, (HeadcountBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of headcount bundle store.</summary>
public static class HeadcountBundleStore
{
    public static IHeadcountBundleStore NewMemoryHeadcountBundleStore()
    {
        return new MemoryHeadcountBundleStore(new Dictionary<string, HeadcountBundle>(), new List<string>());
    }

    public static IHeadcountBundleStore NewCachedHeadcountBundleStore(MemoryHeadcountBundleStore inner, int ttl, IClock clock)
    {
        return new CachedHeadcountBundleStore(inner, HeadcountBundleCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<HeadcountBundle> FindHeadcountBundles(IHeadcountBundleStore store, HeadcountBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
