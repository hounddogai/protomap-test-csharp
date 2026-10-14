namespace Acme.Workforce.Compensation.EquityGrant.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive equity grant targets by code.</summary>
public interface IAdaptiveEquityGrantTargetStore
{
    AdaptiveEquityGrantTarget? Find(string code);
    void Save(AdaptiveEquityGrantTarget item);
    bool Remove(string code);
    List<AdaptiveEquityGrantTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveEquityGrantTargetStore(Dictionary<string, AdaptiveEquityGrantTarget> items, List<string> order) : IAdaptiveEquityGrantTargetStore
{
    public Dictionary<string, AdaptiveEquityGrantTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveEquityGrantTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveEquityGrantTarget item)
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

    public List<AdaptiveEquityGrantTarget> List()
    {
        var outValue = new List<AdaptiveEquityGrantTarget>();
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
public sealed class CachedAdaptiveEquityGrantTargetStore(MemoryAdaptiveEquityGrantTargetStore inner, AdaptiveEquityGrantTargetCache cache, int hits, int misses) : IAdaptiveEquityGrantTargetStore
{
    public MemoryAdaptiveEquityGrantTargetStore Inner { get; set; } = inner;
    public AdaptiveEquityGrantTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveEquityGrantTarget? Find(string code)
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

    public void Save(AdaptiveEquityGrantTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveEquityGrantTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive equity grant targets. Empty fields match all.</summary>
public sealed record AdaptiveEquityGrantTargetQuery(string Country, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveEquityGrantTarget item)
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

    public List<AdaptiveEquityGrantTarget> Apply(List<AdaptiveEquityGrantTarget> items)
    {
        var matched = Collections.Filter(items, (AdaptiveEquityGrantTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive equity grant target store.</summary>
public static class AdaptiveEquityGrantTargetStore
{
    public static IAdaptiveEquityGrantTargetStore NewMemoryAdaptiveEquityGrantTargetStore()
    {
        return new MemoryAdaptiveEquityGrantTargetStore(
            new Dictionary<string, AdaptiveEquityGrantTarget>(),
            new List<string>());
    }

    public static IAdaptiveEquityGrantTargetStore NewCachedAdaptiveEquityGrantTargetStore(MemoryAdaptiveEquityGrantTargetStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveEquityGrantTargetStore(
            inner,
            AdaptiveEquityGrantTargetCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveEquityGrantTarget> FindAdaptiveEquityGrantTargets(IAdaptiveEquityGrantTargetStore store, AdaptiveEquityGrantTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
