namespace Acme.Workforce.Orgdesign.Reorg.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled reorg variances by code.</summary>
public interface IScheduledReorgVarianceStore
{
    ScheduledReorgVariance? Find(string code);
    void Save(ScheduledReorgVariance item);
    bool Remove(string code);
    List<ScheduledReorgVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledReorgVarianceStore(Dictionary<string, ScheduledReorgVariance> items, List<string> order) : IScheduledReorgVarianceStore
{
    public Dictionary<string, ScheduledReorgVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledReorgVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledReorgVariance item)
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

    public List<ScheduledReorgVariance> List()
    {
        var outValue = new List<ScheduledReorgVariance>();
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
public sealed class CachedScheduledReorgVarianceStore(MemoryScheduledReorgVarianceStore inner, ScheduledReorgVarianceCache cache, int hits, int misses) : IScheduledReorgVarianceStore
{
    public MemoryScheduledReorgVarianceStore Inner { get; set; } = inner;
    public ScheduledReorgVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledReorgVariance? Find(string code)
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

    public void Save(ScheduledReorgVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledReorgVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled reorg variances. Empty fields match all.</summary>
public sealed record ScheduledReorgVarianceQuery(string Label, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledReorgVariance item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledReorgVariance> Apply(List<ScheduledReorgVariance> items)
    {
        var matched = Collections.Filter(items, (ScheduledReorgVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled reorg variance store.</summary>
public static class ScheduledReorgVarianceStore
{
    public static IScheduledReorgVarianceStore NewMemoryScheduledReorgVarianceStore()
    {
        return new MemoryScheduledReorgVarianceStore(
            new Dictionary<string, ScheduledReorgVariance>(),
            new List<string>());
    }

    public static IScheduledReorgVarianceStore NewCachedScheduledReorgVarianceStore(MemoryScheduledReorgVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledReorgVarianceStore(
            inner,
            ScheduledReorgVarianceCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledReorgVariance> FindScheduledReorgVariances(IScheduledReorgVarianceStore store, ScheduledReorgVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
