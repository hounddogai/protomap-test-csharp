namespace Acme.Workforce.Absence.Holiday.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient holiday adjustments by code.</summary>
public interface ILenientHolidayAdjustmentStore
{
    LenientHolidayAdjustment? Find(string code);
    void Save(LenientHolidayAdjustment item);
    bool Remove(string code);
    List<LenientHolidayAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientHolidayAdjustmentStore(Dictionary<string, LenientHolidayAdjustment> items, List<string> order) : ILenientHolidayAdjustmentStore
{
    public Dictionary<string, LenientHolidayAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientHolidayAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientHolidayAdjustment item)
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

    public List<LenientHolidayAdjustment> List()
    {
        var outValue = new List<LenientHolidayAdjustment>();
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
public sealed class CachedLenientHolidayAdjustmentStore(MemoryLenientHolidayAdjustmentStore inner, LenientHolidayAdjustmentCache cache, int hits, int misses) : ILenientHolidayAdjustmentStore
{
    public MemoryLenientHolidayAdjustmentStore Inner { get; set; } = inner;
    public LenientHolidayAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientHolidayAdjustment? Find(string code)
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

    public void Save(LenientHolidayAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientHolidayAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient holiday adjustments. Empty fields match all.</summary>
public sealed record LenientHolidayAdjustmentQuery(string Category, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(LenientHolidayAdjustment item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientHolidayAdjustment> Apply(List<LenientHolidayAdjustment> items)
    {
        var matched = Collections.Filter(items, (LenientHolidayAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient holiday adjustment store.</summary>
public static class LenientHolidayAdjustmentStore
{
    public static ILenientHolidayAdjustmentStore NewMemoryLenientHolidayAdjustmentStore()
    {
        return new MemoryLenientHolidayAdjustmentStore(
            new Dictionary<string, LenientHolidayAdjustment>(),
            new List<string>());
    }

    public static ILenientHolidayAdjustmentStore NewCachedLenientHolidayAdjustmentStore(MemoryLenientHolidayAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedLenientHolidayAdjustmentStore(
            inner,
            LenientHolidayAdjustmentCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<LenientHolidayAdjustment> FindLenientHolidayAdjustments(ILenientHolidayAdjustmentStore store, LenientHolidayAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
