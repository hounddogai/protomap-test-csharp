namespace Acme.Workforce.Workplace.FloorPlan.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped floor plan thresholds by code.</summary>
public interface ICappedFloorPlanThresholdStore
{
    CappedFloorPlanThreshold? Find(string code);
    void Save(CappedFloorPlanThreshold item);
    bool Remove(string code);
    List<CappedFloorPlanThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedFloorPlanThresholdStore(Dictionary<string, CappedFloorPlanThreshold> items, List<string> order) : ICappedFloorPlanThresholdStore
{
    public Dictionary<string, CappedFloorPlanThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedFloorPlanThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedFloorPlanThreshold item)
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

    public List<CappedFloorPlanThreshold> List()
    {
        var outValue = new List<CappedFloorPlanThreshold>();
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
public sealed class CachedCappedFloorPlanThresholdStore(MemoryCappedFloorPlanThresholdStore inner, CappedFloorPlanThresholdCache cache, int hits, int misses) : ICappedFloorPlanThresholdStore
{
    public MemoryCappedFloorPlanThresholdStore Inner { get; set; } = inner;
    public CappedFloorPlanThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedFloorPlanThreshold? Find(string code)
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

    public void Save(CappedFloorPlanThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedFloorPlanThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped floor plan thresholds. Empty fields match all.</summary>
public sealed record CappedFloorPlanThresholdQuery(string Segment, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(CappedFloorPlanThreshold item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedFloorPlanThreshold> Apply(List<CappedFloorPlanThreshold> items)
    {
        var matched = Collections.Filter(items, (CappedFloorPlanThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped floor plan threshold store.</summary>
public static class CappedFloorPlanThresholdStore
{
    public static ICappedFloorPlanThresholdStore NewMemoryCappedFloorPlanThresholdStore()
    {
        return new MemoryCappedFloorPlanThresholdStore(
            new Dictionary<string, CappedFloorPlanThreshold>(),
            new List<string>());
    }

    public static ICappedFloorPlanThresholdStore NewCachedCappedFloorPlanThresholdStore(MemoryCappedFloorPlanThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedCappedFloorPlanThresholdStore(
            inner,
            CappedFloorPlanThresholdCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<CappedFloorPlanThreshold> FindCappedFloorPlanThresholds(ICappedFloorPlanThresholdStore store, CappedFloorPlanThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
