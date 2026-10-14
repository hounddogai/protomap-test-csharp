namespace Acme.Workforce.Orgdesign.Reorg.ReorgSnapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered reorg variances by code.</summary>
public interface ITieredReorgVarianceStore
{
    TieredReorgVariance? Find(string code);
    void Save(TieredReorgVariance item);
    bool Remove(string code);
    List<TieredReorgVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredReorgVarianceStore(Dictionary<string, TieredReorgVariance> items, List<string> order) : ITieredReorgVarianceStore
{
    public Dictionary<string, TieredReorgVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredReorgVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredReorgVariance item)
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

    public List<TieredReorgVariance> List()
    {
        var outValue = new List<TieredReorgVariance>();
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
public sealed class CachedTieredReorgVarianceStore(MemoryTieredReorgVarianceStore inner, TieredReorgVarianceCache cache, int hits, int misses) : ITieredReorgVarianceStore
{
    public MemoryTieredReorgVarianceStore Inner { get; set; } = inner;
    public TieredReorgVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredReorgVariance? Find(string code)
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

    public void Save(TieredReorgVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredReorgVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered reorg variances. Empty fields match all.</summary>
public sealed record TieredReorgVarianceQuery(string Segment, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(TieredReorgVariance item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
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

    public List<TieredReorgVariance> Apply(List<TieredReorgVariance> items)
    {
        var matched = Collections.Filter(items, (TieredReorgVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered reorg variance store.</summary>
public static class TieredReorgVarianceStore
{
    public static ITieredReorgVarianceStore NewMemoryTieredReorgVarianceStore()
    {
        return new MemoryTieredReorgVarianceStore(new Dictionary<string, TieredReorgVariance>(), new List<string>());
    }

    public static ITieredReorgVarianceStore NewCachedTieredReorgVarianceStore(MemoryTieredReorgVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedTieredReorgVarianceStore(inner, TieredReorgVarianceCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<TieredReorgVariance> FindTieredReorgVariances(ITieredReorgVarianceStore store, TieredReorgVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
