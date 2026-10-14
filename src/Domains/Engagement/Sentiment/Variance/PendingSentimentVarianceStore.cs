namespace Acme.Workforce.Engagement.Sentiment.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending sentiment variances by code.</summary>
public interface IPendingSentimentVarianceStore
{
    PendingSentimentVariance? Find(string code);
    void Save(PendingSentimentVariance item);
    bool Remove(string code);
    List<PendingSentimentVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingSentimentVarianceStore(Dictionary<string, PendingSentimentVariance> items, List<string> order) : IPendingSentimentVarianceStore
{
    public Dictionary<string, PendingSentimentVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingSentimentVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingSentimentVariance item)
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

    public List<PendingSentimentVariance> List()
    {
        var outValue = new List<PendingSentimentVariance>();
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
public sealed class CachedPendingSentimentVarianceStore(MemoryPendingSentimentVarianceStore inner, PendingSentimentVarianceCache cache, int hits, int misses) : IPendingSentimentVarianceStore
{
    public MemoryPendingSentimentVarianceStore Inner { get; set; } = inner;
    public PendingSentimentVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingSentimentVariance? Find(string code)
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

    public void Save(PendingSentimentVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingSentimentVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending sentiment variances. Empty fields match all.</summary>
public sealed record PendingSentimentVarianceQuery(string Name, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(PendingSentimentVariance item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingSentimentVariance> Apply(List<PendingSentimentVariance> items)
    {
        var matched = Collections.Filter(items, (PendingSentimentVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending sentiment variance store.</summary>
public static class PendingSentimentVarianceStore
{
    public static IPendingSentimentVarianceStore NewMemoryPendingSentimentVarianceStore()
    {
        return new MemoryPendingSentimentVarianceStore(
            new Dictionary<string, PendingSentimentVariance>(),
            new List<string>());
    }

    public static IPendingSentimentVarianceStore NewCachedPendingSentimentVarianceStore(MemoryPendingSentimentVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedPendingSentimentVarianceStore(
            inner,
            PendingSentimentVarianceCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<PendingSentimentVariance> FindPendingSentimentVariances(IPendingSentimentVarianceStore store, PendingSentimentVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
