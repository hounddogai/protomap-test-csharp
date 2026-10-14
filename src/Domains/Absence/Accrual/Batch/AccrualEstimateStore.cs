namespace Acme.Workforce.Absence.Accrual.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps accrual estimates by code.</summary>
public interface IAccrualEstimateStore
{
    AccrualEstimate? Find(string code);
    void Save(AccrualEstimate item);
    bool Remove(string code);
    List<AccrualEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccrualEstimateStore(Dictionary<string, AccrualEstimate> items, List<string> order) : IAccrualEstimateStore
{
    public Dictionary<string, AccrualEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccrualEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccrualEstimate item)
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

    public List<AccrualEstimate> List()
    {
        var outValue = new List<AccrualEstimate>();
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
public sealed class CachedAccrualEstimateStore(MemoryAccrualEstimateStore inner, AccrualEstimateCache cache, int hits, int misses) : IAccrualEstimateStore
{
    public MemoryAccrualEstimateStore Inner { get; set; } = inner;
    public AccrualEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccrualEstimate? Find(string code)
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

    public void Save(AccrualEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccrualEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects accrual estimates. Empty fields match all.</summary>
public sealed record AccrualEstimateQuery(string Name, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(AccrualEstimate item)
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

    public List<AccrualEstimate> Apply(List<AccrualEstimate> items)
    {
        var matched = Collections.Filter(items, (AccrualEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of accrual estimate store.</summary>
public static class AccrualEstimateStore
{
    public static IAccrualEstimateStore NewMemoryAccrualEstimateStore()
    {
        return new MemoryAccrualEstimateStore(new Dictionary<string, AccrualEstimate>(), new List<string>());
    }

    public static IAccrualEstimateStore NewCachedAccrualEstimateStore(MemoryAccrualEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedAccrualEstimateStore(inner, AccrualEstimateCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<AccrualEstimate> FindAccrualEstimates(IAccrualEstimateStore store, AccrualEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
