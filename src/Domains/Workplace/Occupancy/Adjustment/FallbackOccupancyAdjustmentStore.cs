namespace Acme.Workforce.Workplace.Occupancy.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback occupancy adjustments by code.</summary>
public interface IFallbackOccupancyAdjustmentStore
{
    FallbackOccupancyAdjustment? Find(string code);
    void Save(FallbackOccupancyAdjustment item);
    bool Remove(string code);
    List<FallbackOccupancyAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackOccupancyAdjustmentStore(Dictionary<string, FallbackOccupancyAdjustment> items, List<string> order) : IFallbackOccupancyAdjustmentStore
{
    public Dictionary<string, FallbackOccupancyAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackOccupancyAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackOccupancyAdjustment item)
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

    public List<FallbackOccupancyAdjustment> List()
    {
        var outValue = new List<FallbackOccupancyAdjustment>();
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
public sealed class CachedFallbackOccupancyAdjustmentStore(MemoryFallbackOccupancyAdjustmentStore inner, FallbackOccupancyAdjustmentCache cache, int hits, int misses) : IFallbackOccupancyAdjustmentStore
{
    public MemoryFallbackOccupancyAdjustmentStore Inner { get; set; } = inner;
    public FallbackOccupancyAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackOccupancyAdjustment? Find(string code)
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

    public void Save(FallbackOccupancyAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackOccupancyAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback occupancy adjustments. Empty fields match all.</summary>
public sealed record FallbackOccupancyAdjustmentQuery(string Label, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackOccupancyAdjustment item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackOccupancyAdjustment> Apply(List<FallbackOccupancyAdjustment> items)
    {
        var matched = Collections.Filter(items, (FallbackOccupancyAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback occupancy adjustment store.</summary>
public static class FallbackOccupancyAdjustmentStore
{
    public static IFallbackOccupancyAdjustmentStore NewMemoryFallbackOccupancyAdjustmentStore()
    {
        return new MemoryFallbackOccupancyAdjustmentStore(
            new Dictionary<string, FallbackOccupancyAdjustment>(),
            new List<string>());
    }

    public static IFallbackOccupancyAdjustmentStore NewCachedFallbackOccupancyAdjustmentStore(MemoryFallbackOccupancyAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackOccupancyAdjustmentStore(
            inner,
            FallbackOccupancyAdjustmentCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<FallbackOccupancyAdjustment> FindFallbackOccupancyAdjustments(IFallbackOccupancyAdjustmentStore store, FallbackOccupancyAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
