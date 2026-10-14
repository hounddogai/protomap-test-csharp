namespace Acme.Workforce.Workplace.FloorPlan.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy floor plan thresholds by code.</summary>
public interface ILegacyFloorPlanThresholdStore
{
    LegacyFloorPlanThreshold? Find(string code);
    void Save(LegacyFloorPlanThreshold item);
    bool Remove(string code);
    List<LegacyFloorPlanThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyFloorPlanThresholdStore(Dictionary<string, LegacyFloorPlanThreshold> items, List<string> order) : ILegacyFloorPlanThresholdStore
{
    public Dictionary<string, LegacyFloorPlanThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyFloorPlanThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyFloorPlanThreshold item)
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

    public List<LegacyFloorPlanThreshold> List()
    {
        var outValue = new List<LegacyFloorPlanThreshold>();
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
public sealed class CachedLegacyFloorPlanThresholdStore(MemoryLegacyFloorPlanThresholdStore inner, LegacyFloorPlanThresholdCache cache, int hits, int misses) : ILegacyFloorPlanThresholdStore
{
    public MemoryLegacyFloorPlanThresholdStore Inner { get; set; } = inner;
    public LegacyFloorPlanThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyFloorPlanThreshold? Find(string code)
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

    public void Save(LegacyFloorPlanThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyFloorPlanThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy floor plan thresholds. Empty fields match all.</summary>
public sealed record LegacyFloorPlanThresholdQuery(string Category, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyFloorPlanThreshold item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<LegacyFloorPlanThreshold> Apply(List<LegacyFloorPlanThreshold> items)
    {
        var matched = Collections.Filter(items, (LegacyFloorPlanThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy floor plan threshold store.</summary>
public static class LegacyFloorPlanThresholdStore
{
    public static ILegacyFloorPlanThresholdStore NewMemoryLegacyFloorPlanThresholdStore()
    {
        return new MemoryLegacyFloorPlanThresholdStore(
            new Dictionary<string, LegacyFloorPlanThreshold>(),
            new List<string>());
    }

    public static ILegacyFloorPlanThresholdStore NewCachedLegacyFloorPlanThresholdStore(MemoryLegacyFloorPlanThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyFloorPlanThresholdStore(
            inner,
            LegacyFloorPlanThresholdCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<LegacyFloorPlanThreshold> FindLegacyFloorPlanThresholds(ILegacyFloorPlanThresholdStore store, LegacyFloorPlanThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
