namespace Acme.Workforce.Orgdesign.Reorg.ReorgSummary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending reorg variances by code.</summary>
public interface IPendingReorgVarianceStore
{
    PendingReorgVariance? Find(string code);
    void Save(PendingReorgVariance item);
    bool Remove(string code);
    List<PendingReorgVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingReorgVarianceStore(Dictionary<string, PendingReorgVariance> items, List<string> order) : IPendingReorgVarianceStore
{
    public Dictionary<string, PendingReorgVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingReorgVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingReorgVariance item)
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

    public List<PendingReorgVariance> List()
    {
        var outValue = new List<PendingReorgVariance>();
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
public sealed class CachedPendingReorgVarianceStore(MemoryPendingReorgVarianceStore inner, PendingReorgVarianceCache cache, int hits, int misses) : IPendingReorgVarianceStore
{
    public MemoryPendingReorgVarianceStore Inner { get; set; } = inner;
    public PendingReorgVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingReorgVariance? Find(string code)
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

    public void Save(PendingReorgVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingReorgVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending reorg variances. Empty fields match all.</summary>
public sealed record PendingReorgVarianceQuery(string Name, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(PendingReorgVariance item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingReorgVariance> Apply(List<PendingReorgVariance> items)
    {
        var matched = Collections.Filter(items, (PendingReorgVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending reorg variance store.</summary>
public static class PendingReorgVarianceStore
{
    public static IPendingReorgVarianceStore NewMemoryPendingReorgVarianceStore()
    {
        return new MemoryPendingReorgVarianceStore(
            new Dictionary<string, PendingReorgVariance>(),
            new List<string>());
    }

    public static IPendingReorgVarianceStore NewCachedPendingReorgVarianceStore(MemoryPendingReorgVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedPendingReorgVarianceStore(inner, PendingReorgVarianceCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PendingReorgVariance> FindPendingReorgVariances(IPendingReorgVarianceStore store, PendingReorgVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
