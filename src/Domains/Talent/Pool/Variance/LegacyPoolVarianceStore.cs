namespace Acme.Workforce.Talent.Pool.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy pool variances by code.</summary>
public interface ILegacyPoolVarianceStore
{
    LegacyPoolVariance? Find(string code);
    void Save(LegacyPoolVariance item);
    bool Remove(string code);
    List<LegacyPoolVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyPoolVarianceStore(Dictionary<string, LegacyPoolVariance> items, List<string> order) : ILegacyPoolVarianceStore
{
    public Dictionary<string, LegacyPoolVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyPoolVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyPoolVariance item)
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

    public List<LegacyPoolVariance> List()
    {
        var outValue = new List<LegacyPoolVariance>();
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
public sealed class CachedLegacyPoolVarianceStore(MemoryLegacyPoolVarianceStore inner, LegacyPoolVarianceCache cache, int hits, int misses) : ILegacyPoolVarianceStore
{
    public MemoryLegacyPoolVarianceStore Inner { get; set; } = inner;
    public LegacyPoolVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyPoolVariance? Find(string code)
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

    public void Save(LegacyPoolVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyPoolVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy pool variances. Empty fields match all.</summary>
public sealed record LegacyPoolVarianceQuery(string Title, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyPoolVariance item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<LegacyPoolVariance> Apply(List<LegacyPoolVariance> items)
    {
        var matched = Collections.Filter(items, (LegacyPoolVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy pool variance store.</summary>
public static class LegacyPoolVarianceStore
{
    public static ILegacyPoolVarianceStore NewMemoryLegacyPoolVarianceStore()
    {
        return new MemoryLegacyPoolVarianceStore(new Dictionary<string, LegacyPoolVariance>(), new List<string>());
    }

    public static ILegacyPoolVarianceStore NewCachedLegacyPoolVarianceStore(MemoryLegacyPoolVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyPoolVarianceStore(inner, LegacyPoolVarianceCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<LegacyPoolVariance> FindLegacyPoolVariances(ILegacyPoolVarianceStore store, LegacyPoolVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
