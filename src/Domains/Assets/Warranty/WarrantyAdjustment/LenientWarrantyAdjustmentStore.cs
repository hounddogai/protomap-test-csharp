namespace Acme.Workforce.Assets.Warranty.WarrantyAdjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient warranty adjustments by code.</summary>
public interface ILenientWarrantyAdjustmentStore
{
    LenientWarrantyAdjustment? Find(string code);
    void Save(LenientWarrantyAdjustment item);
    bool Remove(string code);
    List<LenientWarrantyAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientWarrantyAdjustmentStore(Dictionary<string, LenientWarrantyAdjustment> items, List<string> order) : ILenientWarrantyAdjustmentStore
{
    public Dictionary<string, LenientWarrantyAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientWarrantyAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientWarrantyAdjustment item)
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

    public List<LenientWarrantyAdjustment> List()
    {
        var outValue = new List<LenientWarrantyAdjustment>();
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
public sealed class CachedLenientWarrantyAdjustmentStore(MemoryLenientWarrantyAdjustmentStore inner, LenientWarrantyAdjustmentCache cache, int hits, int misses) : ILenientWarrantyAdjustmentStore
{
    public MemoryLenientWarrantyAdjustmentStore Inner { get; set; } = inner;
    public LenientWarrantyAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientWarrantyAdjustment? Find(string code)
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

    public void Save(LenientWarrantyAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientWarrantyAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient warranty adjustments. Empty fields match all.</summary>
public sealed record LenientWarrantyAdjustmentQuery(string Title, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(LenientWarrantyAdjustment item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<LenientWarrantyAdjustment> Apply(List<LenientWarrantyAdjustment> items)
    {
        var matched = Collections.Filter(items, (LenientWarrantyAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient warranty adjustment store.</summary>
public static class LenientWarrantyAdjustmentStore
{
    public static ILenientWarrantyAdjustmentStore NewMemoryLenientWarrantyAdjustmentStore()
    {
        return new MemoryLenientWarrantyAdjustmentStore(
            new Dictionary<string, LenientWarrantyAdjustment>(),
            new List<string>());
    }

    public static ILenientWarrantyAdjustmentStore NewCachedLenientWarrantyAdjustmentStore(MemoryLenientWarrantyAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedLenientWarrantyAdjustmentStore(
            inner,
            LenientWarrantyAdjustmentCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<LenientWarrantyAdjustment> FindLenientWarrantyAdjustments(ILenientWarrantyAdjustmentStore store, LenientWarrantyAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
