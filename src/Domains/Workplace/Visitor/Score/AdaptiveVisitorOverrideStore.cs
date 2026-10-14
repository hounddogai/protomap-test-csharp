namespace Acme.Workforce.Workplace.Visitor.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive visitor overrides by code.</summary>
public interface IAdaptiveVisitorOverrideStore
{
    AdaptiveVisitorOverride? Find(string code);
    void Save(AdaptiveVisitorOverride item);
    bool Remove(string code);
    List<AdaptiveVisitorOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveVisitorOverrideStore(Dictionary<string, AdaptiveVisitorOverride> items, List<string> order) : IAdaptiveVisitorOverrideStore
{
    public Dictionary<string, AdaptiveVisitorOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveVisitorOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveVisitorOverride item)
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

    public List<AdaptiveVisitorOverride> List()
    {
        var outValue = new List<AdaptiveVisitorOverride>();
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
public sealed class CachedAdaptiveVisitorOverrideStore(MemoryAdaptiveVisitorOverrideStore inner, AdaptiveVisitorOverrideCache cache, int hits, int misses) : IAdaptiveVisitorOverrideStore
{
    public MemoryAdaptiveVisitorOverrideStore Inner { get; set; } = inner;
    public AdaptiveVisitorOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveVisitorOverride? Find(string code)
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

    public void Save(AdaptiveVisitorOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveVisitorOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive visitor overrides. Empty fields match all.</summary>
public sealed record AdaptiveVisitorOverrideQuery(string Category, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveVisitorOverride item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<AdaptiveVisitorOverride> Apply(List<AdaptiveVisitorOverride> items)
    {
        var matched = Collections.Filter(items, (AdaptiveVisitorOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive visitor override store.</summary>
public static class AdaptiveVisitorOverrideStore
{
    public static IAdaptiveVisitorOverrideStore NewMemoryAdaptiveVisitorOverrideStore()
    {
        return new MemoryAdaptiveVisitorOverrideStore(
            new Dictionary<string, AdaptiveVisitorOverride>(),
            new List<string>());
    }

    public static IAdaptiveVisitorOverrideStore NewCachedAdaptiveVisitorOverrideStore(MemoryAdaptiveVisitorOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveVisitorOverrideStore(
            inner,
            AdaptiveVisitorOverrideCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveVisitorOverride> FindAdaptiveVisitorOverrides(IAdaptiveVisitorOverrideStore store, AdaptiveVisitorOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
