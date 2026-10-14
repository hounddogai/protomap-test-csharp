namespace Acme.Workforce.Contractors.Rate.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback rate bundles by code.</summary>
public interface IFallbackRateBundleStore
{
    FallbackRateBundle? Find(string code);
    void Save(FallbackRateBundle item);
    bool Remove(string code);
    List<FallbackRateBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackRateBundleStore(Dictionary<string, FallbackRateBundle> items, List<string> order) : IFallbackRateBundleStore
{
    public Dictionary<string, FallbackRateBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackRateBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackRateBundle item)
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

    public List<FallbackRateBundle> List()
    {
        var outValue = new List<FallbackRateBundle>();
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
public sealed class CachedFallbackRateBundleStore(MemoryFallbackRateBundleStore inner, FallbackRateBundleCache cache, int hits, int misses) : IFallbackRateBundleStore
{
    public MemoryFallbackRateBundleStore Inner { get; set; } = inner;
    public FallbackRateBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackRateBundle? Find(string code)
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

    public void Save(FallbackRateBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackRateBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback rate bundles. Empty fields match all.</summary>
public sealed record FallbackRateBundleQuery(string Team, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackRateBundle item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<FallbackRateBundle> Apply(List<FallbackRateBundle> items)
    {
        var matched = Collections.Filter(items, (FallbackRateBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback rate bundle store.</summary>
public static class FallbackRateBundleStore
{
    public static IFallbackRateBundleStore NewMemoryFallbackRateBundleStore()
    {
        return new MemoryFallbackRateBundleStore(new Dictionary<string, FallbackRateBundle>(), new List<string>());
    }

    public static IFallbackRateBundleStore NewCachedFallbackRateBundleStore(MemoryFallbackRateBundleStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackRateBundleStore(inner, FallbackRateBundleCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<FallbackRateBundle> FindFallbackRateBundles(IFallbackRateBundleStore store, FallbackRateBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
