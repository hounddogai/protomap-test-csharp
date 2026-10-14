namespace Acme.Workforce.Workplace.Occupancy.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps occupancy adjustments by code.</summary>
public interface IOccupancyAdjustmentStore
{
    OccupancyAdjustment? Find(string code);
    void Save(OccupancyAdjustment item);
    bool Remove(string code);
    List<OccupancyAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOccupancyAdjustmentStore(Dictionary<string, OccupancyAdjustment> items, List<string> order) : IOccupancyAdjustmentStore
{
    public Dictionary<string, OccupancyAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OccupancyAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OccupancyAdjustment item)
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

    public List<OccupancyAdjustment> List()
    {
        var outValue = new List<OccupancyAdjustment>();
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
public sealed class CachedOccupancyAdjustmentStore(MemoryOccupancyAdjustmentStore inner, OccupancyAdjustmentCache cache, int hits, int misses) : IOccupancyAdjustmentStore
{
    public MemoryOccupancyAdjustmentStore Inner { get; set; } = inner;
    public OccupancyAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OccupancyAdjustment? Find(string code)
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

    public void Save(OccupancyAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OccupancyAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects occupancy adjustments. Empty fields match all.</summary>
public sealed record OccupancyAdjustmentQuery(string Title, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(OccupancyAdjustment item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<OccupancyAdjustment> Apply(List<OccupancyAdjustment> items)
    {
        var matched = Collections.Filter(items, (OccupancyAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of occupancy adjustment store.</summary>
public static class OccupancyAdjustmentStore
{
    public static IOccupancyAdjustmentStore NewMemoryOccupancyAdjustmentStore()
    {
        return new MemoryOccupancyAdjustmentStore(new Dictionary<string, OccupancyAdjustment>(), new List<string>());
    }

    public static IOccupancyAdjustmentStore NewCachedOccupancyAdjustmentStore(MemoryOccupancyAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedOccupancyAdjustmentStore(inner, OccupancyAdjustmentCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<OccupancyAdjustment> FindOccupancyAdjustments(IOccupancyAdjustmentStore store, OccupancyAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
