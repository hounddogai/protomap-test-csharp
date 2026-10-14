namespace Acme.Workforce.Workplace.Desk.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient desk adjustments by code.</summary>
public interface ILenientDeskAdjustmentStore
{
    LenientDeskAdjustment? Find(string code);
    void Save(LenientDeskAdjustment item);
    bool Remove(string code);
    List<LenientDeskAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientDeskAdjustmentStore(Dictionary<string, LenientDeskAdjustment> items, List<string> order) : ILenientDeskAdjustmentStore
{
    public Dictionary<string, LenientDeskAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientDeskAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientDeskAdjustment item)
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

    public List<LenientDeskAdjustment> List()
    {
        var outValue = new List<LenientDeskAdjustment>();
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
public sealed class CachedLenientDeskAdjustmentStore(MemoryLenientDeskAdjustmentStore inner, LenientDeskAdjustmentCache cache, int hits, int misses) : ILenientDeskAdjustmentStore
{
    public MemoryLenientDeskAdjustmentStore Inner { get; set; } = inner;
    public LenientDeskAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientDeskAdjustment? Find(string code)
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

    public void Save(LenientDeskAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientDeskAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient desk adjustments. Empty fields match all.</summary>
public sealed record LenientDeskAdjustmentQuery(string Label, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(LenientDeskAdjustment item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientDeskAdjustment> Apply(List<LenientDeskAdjustment> items)
    {
        var matched = Collections.Filter(items, (LenientDeskAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient desk adjustment store.</summary>
public static class LenientDeskAdjustmentStore
{
    public static ILenientDeskAdjustmentStore NewMemoryLenientDeskAdjustmentStore()
    {
        return new MemoryLenientDeskAdjustmentStore(
            new Dictionary<string, LenientDeskAdjustment>(),
            new List<string>());
    }

    public static ILenientDeskAdjustmentStore NewCachedLenientDeskAdjustmentStore(MemoryLenientDeskAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedLenientDeskAdjustmentStore(inner, LenientDeskAdjustmentCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<LenientDeskAdjustment> FindLenientDeskAdjustments(ILenientDeskAdjustmentStore store, LenientDeskAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
