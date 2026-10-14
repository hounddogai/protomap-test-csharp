namespace Acme.Workforce.Compensation.Benchmark.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending benchmark adjustments by code.</summary>
public interface IPendingBenchmarkAdjustmentStore
{
    PendingBenchmarkAdjustment? Find(string code);
    void Save(PendingBenchmarkAdjustment item);
    bool Remove(string code);
    List<PendingBenchmarkAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingBenchmarkAdjustmentStore(Dictionary<string, PendingBenchmarkAdjustment> items, List<string> order) : IPendingBenchmarkAdjustmentStore
{
    public Dictionary<string, PendingBenchmarkAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingBenchmarkAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingBenchmarkAdjustment item)
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

    public List<PendingBenchmarkAdjustment> List()
    {
        var outValue = new List<PendingBenchmarkAdjustment>();
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
public sealed class CachedPendingBenchmarkAdjustmentStore(MemoryPendingBenchmarkAdjustmentStore inner, PendingBenchmarkAdjustmentCache cache, int hits, int misses) : IPendingBenchmarkAdjustmentStore
{
    public MemoryPendingBenchmarkAdjustmentStore Inner { get; set; } = inner;
    public PendingBenchmarkAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingBenchmarkAdjustment? Find(string code)
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

    public void Save(PendingBenchmarkAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingBenchmarkAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending benchmark adjustments. Empty fields match all.</summary>
public sealed record PendingBenchmarkAdjustmentQuery(string Channel, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(PendingBenchmarkAdjustment item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingBenchmarkAdjustment> Apply(List<PendingBenchmarkAdjustment> items)
    {
        var matched = Collections.Filter(items, (PendingBenchmarkAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending benchmark adjustment store.</summary>
public static class PendingBenchmarkAdjustmentStore
{
    public static IPendingBenchmarkAdjustmentStore NewMemoryPendingBenchmarkAdjustmentStore()
    {
        return new MemoryPendingBenchmarkAdjustmentStore(
            new Dictionary<string, PendingBenchmarkAdjustment>(),
            new List<string>());
    }

    public static IPendingBenchmarkAdjustmentStore NewCachedPendingBenchmarkAdjustmentStore(MemoryPendingBenchmarkAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedPendingBenchmarkAdjustmentStore(
            inner,
            PendingBenchmarkAdjustmentCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<PendingBenchmarkAdjustment> FindPendingBenchmarkAdjustments(IPendingBenchmarkAdjustmentStore store, PendingBenchmarkAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
