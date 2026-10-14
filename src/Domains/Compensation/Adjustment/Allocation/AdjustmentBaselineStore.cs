namespace Acme.Workforce.Compensation.Adjustment.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adjustment baselines by code.</summary>
public interface IAdjustmentBaselineStore
{
    AdjustmentBaseline? Find(string code);
    void Save(AdjustmentBaseline item);
    bool Remove(string code);
    List<AdjustmentBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdjustmentBaselineStore(Dictionary<string, AdjustmentBaseline> items, List<string> order) : IAdjustmentBaselineStore
{
    public Dictionary<string, AdjustmentBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdjustmentBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdjustmentBaseline item)
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

    public List<AdjustmentBaseline> List()
    {
        var outValue = new List<AdjustmentBaseline>();
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
public sealed class CachedAdjustmentBaselineStore(MemoryAdjustmentBaselineStore inner, AdjustmentBaselineCache cache, int hits, int misses) : IAdjustmentBaselineStore
{
    public MemoryAdjustmentBaselineStore Inner { get; set; } = inner;
    public AdjustmentBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdjustmentBaseline? Find(string code)
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

    public void Save(AdjustmentBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdjustmentBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adjustment baselines. Empty fields match all.</summary>
public sealed record AdjustmentBaselineQuery(string Title, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(AdjustmentBaseline item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<AdjustmentBaseline> Apply(List<AdjustmentBaseline> items)
    {
        var matched = Collections.Filter(items, (AdjustmentBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adjustment baseline store.</summary>
public static class AdjustmentBaselineStore
{
    public static IAdjustmentBaselineStore NewMemoryAdjustmentBaselineStore()
    {
        return new MemoryAdjustmentBaselineStore(new Dictionary<string, AdjustmentBaseline>(), new List<string>());
    }

    public static IAdjustmentBaselineStore NewCachedAdjustmentBaselineStore(MemoryAdjustmentBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedAdjustmentBaselineStore(inner, AdjustmentBaselineCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<AdjustmentBaseline> FindAdjustmentBaselines(IAdjustmentBaselineStore store, AdjustmentBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
