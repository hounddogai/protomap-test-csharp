namespace Acme.Workforce.Contractors.Engagement.Manifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps engagement variances by code.</summary>
public interface IEngagementVarianceStore
{
    EngagementVariance? Find(string code);
    void Save(EngagementVariance item);
    bool Remove(string code);
    List<EngagementVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryEngagementVarianceStore(Dictionary<string, EngagementVariance> items, List<string> order) : IEngagementVarianceStore
{
    public Dictionary<string, EngagementVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public EngagementVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(EngagementVariance item)
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

    public List<EngagementVariance> List()
    {
        var outValue = new List<EngagementVariance>();
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
public sealed class CachedEngagementVarianceStore(MemoryEngagementVarianceStore inner, EngagementVarianceCache cache, int hits, int misses) : IEngagementVarianceStore
{
    public MemoryEngagementVarianceStore Inner { get; set; } = inner;
    public EngagementVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public EngagementVariance? Find(string code)
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

    public void Save(EngagementVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<EngagementVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects engagement variances. Empty fields match all.</summary>
public sealed record EngagementVarianceQuery(string Unit, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(EngagementVariance item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<EngagementVariance> Apply(List<EngagementVariance> items)
    {
        var matched = Collections.Filter(items, (EngagementVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of engagement variance store.</summary>
public static class EngagementVarianceStore
{
    public static IEngagementVarianceStore NewMemoryEngagementVarianceStore()
    {
        return new MemoryEngagementVarianceStore(new Dictionary<string, EngagementVariance>(), new List<string>());
    }

    public static IEngagementVarianceStore NewCachedEngagementVarianceStore(MemoryEngagementVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedEngagementVarianceStore(inner, EngagementVarianceCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<EngagementVariance> FindEngagementVariances(IEngagementVarianceStore store, EngagementVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
