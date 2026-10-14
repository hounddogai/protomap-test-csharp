namespace Acme.Workforce.Compensation.BonusPool.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps bonus pool estimates by code.</summary>
public interface IBonusPoolEstimateStore
{
    BonusPoolEstimate? Find(string code);
    void Save(BonusPoolEstimate item);
    bool Remove(string code);
    List<BonusPoolEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBonusPoolEstimateStore(Dictionary<string, BonusPoolEstimate> items, List<string> order) : IBonusPoolEstimateStore
{
    public Dictionary<string, BonusPoolEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BonusPoolEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BonusPoolEstimate item)
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

    public List<BonusPoolEstimate> List()
    {
        var outValue = new List<BonusPoolEstimate>();
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
public sealed class CachedBonusPoolEstimateStore(MemoryBonusPoolEstimateStore inner, BonusPoolEstimateCache cache, int hits, int misses) : IBonusPoolEstimateStore
{
    public MemoryBonusPoolEstimateStore Inner { get; set; } = inner;
    public BonusPoolEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BonusPoolEstimate? Find(string code)
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

    public void Save(BonusPoolEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BonusPoolEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects bonus pool estimates. Empty fields match all.</summary>
public sealed record BonusPoolEstimateQuery(string Name, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(BonusPoolEstimate item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BonusPoolEstimate> Apply(List<BonusPoolEstimate> items)
    {
        var matched = Collections.Filter(items, (BonusPoolEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of bonus pool estimate store.</summary>
public static class BonusPoolEstimateStore
{
    public static IBonusPoolEstimateStore NewMemoryBonusPoolEstimateStore()
    {
        return new MemoryBonusPoolEstimateStore(new Dictionary<string, BonusPoolEstimate>(), new List<string>());
    }

    public static IBonusPoolEstimateStore NewCachedBonusPoolEstimateStore(MemoryBonusPoolEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedBonusPoolEstimateStore(inner, BonusPoolEstimateCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<BonusPoolEstimate> FindBonusPoolEstimates(IBonusPoolEstimateStore store, BonusPoolEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
