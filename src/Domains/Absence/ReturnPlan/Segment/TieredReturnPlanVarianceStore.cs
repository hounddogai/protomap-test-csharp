namespace Acme.Workforce.Absence.ReturnPlan.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered return plan variances by code.</summary>
public interface ITieredReturnPlanVarianceStore
{
    TieredReturnPlanVariance? Find(string code);
    void Save(TieredReturnPlanVariance item);
    bool Remove(string code);
    List<TieredReturnPlanVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredReturnPlanVarianceStore(Dictionary<string, TieredReturnPlanVariance> items, List<string> order) : ITieredReturnPlanVarianceStore
{
    public Dictionary<string, TieredReturnPlanVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredReturnPlanVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredReturnPlanVariance item)
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

    public List<TieredReturnPlanVariance> List()
    {
        var outValue = new List<TieredReturnPlanVariance>();
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
public sealed class CachedTieredReturnPlanVarianceStore(MemoryTieredReturnPlanVarianceStore inner, TieredReturnPlanVarianceCache cache, int hits, int misses) : ITieredReturnPlanVarianceStore
{
    public MemoryTieredReturnPlanVarianceStore Inner { get; set; } = inner;
    public TieredReturnPlanVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredReturnPlanVariance? Find(string code)
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

    public void Save(TieredReturnPlanVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredReturnPlanVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered return plan variances. Empty fields match all.</summary>
public sealed record TieredReturnPlanVarianceQuery(string Channel, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(TieredReturnPlanVariance item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredReturnPlanVariance> Apply(List<TieredReturnPlanVariance> items)
    {
        var matched = Collections.Filter(items, (TieredReturnPlanVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered return plan variance store.</summary>
public static class TieredReturnPlanVarianceStore
{
    public static ITieredReturnPlanVarianceStore NewMemoryTieredReturnPlanVarianceStore()
    {
        return new MemoryTieredReturnPlanVarianceStore(
            new Dictionary<string, TieredReturnPlanVariance>(),
            new List<string>());
    }

    public static ITieredReturnPlanVarianceStore NewCachedTieredReturnPlanVarianceStore(MemoryTieredReturnPlanVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedTieredReturnPlanVarianceStore(
            inner,
            TieredReturnPlanVarianceCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<TieredReturnPlanVariance> FindTieredReturnPlanVariances(ITieredReturnPlanVarianceStore store, TieredReturnPlanVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
