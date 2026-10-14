namespace Acme.Workforce.Orgdesign.Reorg.StrictReorgBaseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps reorg variances by code.</summary>
public interface IReorgVarianceStore
{
    ReorgVariance? Find(string code);
    void Save(ReorgVariance item);
    bool Remove(string code);
    List<ReorgVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReorgVarianceStore(Dictionary<string, ReorgVariance> items, List<string> order) : IReorgVarianceStore
{
    public Dictionary<string, ReorgVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReorgVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReorgVariance item)
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

    public List<ReorgVariance> List()
    {
        var outValue = new List<ReorgVariance>();
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
public sealed class CachedReorgVarianceStore(MemoryReorgVarianceStore inner, ReorgVarianceCache cache, int hits, int misses) : IReorgVarianceStore
{
    public MemoryReorgVarianceStore Inner { get; set; } = inner;
    public ReorgVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReorgVariance? Find(string code)
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

    public void Save(ReorgVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReorgVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects reorg variances. Empty fields match all.</summary>
public sealed record ReorgVarianceQuery(string Title, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(ReorgVariance item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ReorgVariance> Apply(List<ReorgVariance> items)
    {
        var matched = Collections.Filter(items, (ReorgVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of reorg variance store.</summary>
public static class ReorgVarianceStore
{
    public static IReorgVarianceStore NewMemoryReorgVarianceStore()
    {
        return new MemoryReorgVarianceStore(new Dictionary<string, ReorgVariance>(), new List<string>());
    }

    public static IReorgVarianceStore NewCachedReorgVarianceStore(MemoryReorgVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedReorgVarianceStore(inner, ReorgVarianceCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<ReorgVariance> FindReorgVariances(IReorgVarianceStore store, ReorgVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
