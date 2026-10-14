namespace Acme.Workforce.Orgdesign.Reorg.ReorgPlan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual reorg variances by code.</summary>
public interface IManualReorgVarianceStore
{
    ManualReorgVariance? Find(string code);
    void Save(ManualReorgVariance item);
    bool Remove(string code);
    List<ManualReorgVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualReorgVarianceStore(Dictionary<string, ManualReorgVariance> items, List<string> order) : IManualReorgVarianceStore
{
    public Dictionary<string, ManualReorgVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualReorgVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualReorgVariance item)
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

    public List<ManualReorgVariance> List()
    {
        var outValue = new List<ManualReorgVariance>();
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
public sealed class CachedManualReorgVarianceStore(MemoryManualReorgVarianceStore inner, ManualReorgVarianceCache cache, int hits, int misses) : IManualReorgVarianceStore
{
    public MemoryManualReorgVarianceStore Inner { get; set; } = inner;
    public ManualReorgVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualReorgVariance? Find(string code)
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

    public void Save(ManualReorgVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualReorgVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual reorg variances. Empty fields match all.</summary>
public sealed record ManualReorgVarianceQuery(string Name, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(ManualReorgVariance item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualReorgVariance> Apply(List<ManualReorgVariance> items)
    {
        var matched = Collections.Filter(items, (ManualReorgVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual reorg variance store.</summary>
public static class ManualReorgVarianceStore
{
    public static IManualReorgVarianceStore NewMemoryManualReorgVarianceStore()
    {
        return new MemoryManualReorgVarianceStore(new Dictionary<string, ManualReorgVariance>(), new List<string>());
    }

    public static IManualReorgVarianceStore NewCachedManualReorgVarianceStore(MemoryManualReorgVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedManualReorgVarianceStore(inner, ManualReorgVarianceCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<ManualReorgVariance> FindManualReorgVariances(IManualReorgVarianceStore store, ManualReorgVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
