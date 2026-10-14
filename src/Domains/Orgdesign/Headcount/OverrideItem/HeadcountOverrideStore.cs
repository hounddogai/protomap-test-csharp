namespace Acme.Workforce.Orgdesign.Headcount.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps headcount overrides by code.</summary>
public interface IHeadcountOverrideStore
{
    HeadcountOverride? Find(string code);
    void Save(HeadcountOverride item);
    bool Remove(string code);
    List<HeadcountOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHeadcountOverrideStore(Dictionary<string, HeadcountOverride> items, List<string> order) : IHeadcountOverrideStore
{
    public Dictionary<string, HeadcountOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HeadcountOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HeadcountOverride item)
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

    public List<HeadcountOverride> List()
    {
        var outValue = new List<HeadcountOverride>();
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
public sealed class CachedHeadcountOverrideStore(MemoryHeadcountOverrideStore inner, HeadcountOverrideCache cache, int hits, int misses) : IHeadcountOverrideStore
{
    public MemoryHeadcountOverrideStore Inner { get; set; } = inner;
    public HeadcountOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HeadcountOverride? Find(string code)
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

    public void Save(HeadcountOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HeadcountOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects headcount overrides. Empty fields match all.</summary>
public sealed record HeadcountOverrideQuery(string Country, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(HeadcountOverride item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HeadcountOverride> Apply(List<HeadcountOverride> items)
    {
        var matched = Collections.Filter(items, (HeadcountOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of headcount override store.</summary>
public static class HeadcountOverrideStore
{
    public static IHeadcountOverrideStore NewMemoryHeadcountOverrideStore()
    {
        return new MemoryHeadcountOverrideStore(new Dictionary<string, HeadcountOverride>(), new List<string>());
    }

    public static IHeadcountOverrideStore NewCachedHeadcountOverrideStore(MemoryHeadcountOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedHeadcountOverrideStore(inner, HeadcountOverrideCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<HeadcountOverride> FindHeadcountOverrides(IHeadcountOverrideStore store, HeadcountOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
