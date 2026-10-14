namespace Acme.Workforce.Compensation.EquityGrant.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive equity grant overrides by code.</summary>
public interface IAdaptiveEquityGrantOverrideStore
{
    AdaptiveEquityGrantOverride? Find(string code);
    void Save(AdaptiveEquityGrantOverride item);
    bool Remove(string code);
    List<AdaptiveEquityGrantOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveEquityGrantOverrideStore(Dictionary<string, AdaptiveEquityGrantOverride> items, List<string> order) : IAdaptiveEquityGrantOverrideStore
{
    public Dictionary<string, AdaptiveEquityGrantOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveEquityGrantOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveEquityGrantOverride item)
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

    public List<AdaptiveEquityGrantOverride> List()
    {
        var outValue = new List<AdaptiveEquityGrantOverride>();
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
public sealed class CachedAdaptiveEquityGrantOverrideStore(MemoryAdaptiveEquityGrantOverrideStore inner, AdaptiveEquityGrantOverrideCache cache, int hits, int misses) : IAdaptiveEquityGrantOverrideStore
{
    public MemoryAdaptiveEquityGrantOverrideStore Inner { get; set; } = inner;
    public AdaptiveEquityGrantOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveEquityGrantOverride? Find(string code)
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

    public void Save(AdaptiveEquityGrantOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveEquityGrantOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive equity grant overrides. Empty fields match all.</summary>
public sealed record AdaptiveEquityGrantOverrideQuery(string Segment, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveEquityGrantOverride item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveEquityGrantOverride> Apply(List<AdaptiveEquityGrantOverride> items)
    {
        var matched = Collections.Filter(items, (AdaptiveEquityGrantOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive equity grant override store.</summary>
public static class AdaptiveEquityGrantOverrideStore
{
    public static IAdaptiveEquityGrantOverrideStore NewMemoryAdaptiveEquityGrantOverrideStore()
    {
        return new MemoryAdaptiveEquityGrantOverrideStore(
            new Dictionary<string, AdaptiveEquityGrantOverride>(),
            new List<string>());
    }

    public static IAdaptiveEquityGrantOverrideStore NewCachedAdaptiveEquityGrantOverrideStore(MemoryAdaptiveEquityGrantOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveEquityGrantOverrideStore(
            inner,
            AdaptiveEquityGrantOverrideCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveEquityGrantOverride> FindAdaptiveEquityGrantOverrides(IAdaptiveEquityGrantOverrideStore store, AdaptiveEquityGrantOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
