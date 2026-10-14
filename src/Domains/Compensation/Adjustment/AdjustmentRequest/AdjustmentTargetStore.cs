namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRequest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adjustment targets by code.</summary>
public interface IAdjustmentTargetStore
{
    AdjustmentTarget? Find(string code);
    void Save(AdjustmentTarget item);
    bool Remove(string code);
    List<AdjustmentTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdjustmentTargetStore(Dictionary<string, AdjustmentTarget> items, List<string> order) : IAdjustmentTargetStore
{
    public Dictionary<string, AdjustmentTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdjustmentTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdjustmentTarget item)
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

    public List<AdjustmentTarget> List()
    {
        var outValue = new List<AdjustmentTarget>();
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
public sealed class CachedAdjustmentTargetStore(MemoryAdjustmentTargetStore inner, AdjustmentTargetCache cache, int hits, int misses) : IAdjustmentTargetStore
{
    public MemoryAdjustmentTargetStore Inner { get; set; } = inner;
    public AdjustmentTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdjustmentTarget? Find(string code)
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

    public void Save(AdjustmentTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdjustmentTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adjustment targets. Empty fields match all.</summary>
public sealed record AdjustmentTargetQuery(string Department, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(AdjustmentTarget item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdjustmentTarget> Apply(List<AdjustmentTarget> items)
    {
        var matched = Collections.Filter(items, (AdjustmentTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adjustment target store.</summary>
public static class AdjustmentTargetStore
{
    public static IAdjustmentTargetStore NewMemoryAdjustmentTargetStore()
    {
        return new MemoryAdjustmentTargetStore(new Dictionary<string, AdjustmentTarget>(), new List<string>());
    }

    public static IAdjustmentTargetStore NewCachedAdjustmentTargetStore(MemoryAdjustmentTargetStore inner, int ttl, IClock clock)
    {
        return new CachedAdjustmentTargetStore(inner, AdjustmentTargetCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<AdjustmentTarget> FindAdjustmentTargets(IAdjustmentTargetStore store, AdjustmentTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
