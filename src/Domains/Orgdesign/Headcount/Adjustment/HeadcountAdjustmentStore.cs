namespace Acme.Workforce.Orgdesign.Headcount.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps headcount adjustments by code.</summary>
public interface IHeadcountAdjustmentStore
{
    HeadcountAdjustment? Find(string code);
    void Save(HeadcountAdjustment item);
    bool Remove(string code);
    List<HeadcountAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHeadcountAdjustmentStore(Dictionary<string, HeadcountAdjustment> items, List<string> order) : IHeadcountAdjustmentStore
{
    public Dictionary<string, HeadcountAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HeadcountAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HeadcountAdjustment item)
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

    public List<HeadcountAdjustment> List()
    {
        var outValue = new List<HeadcountAdjustment>();
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
public sealed class CachedHeadcountAdjustmentStore(MemoryHeadcountAdjustmentStore inner, HeadcountAdjustmentCache cache, int hits, int misses) : IHeadcountAdjustmentStore
{
    public MemoryHeadcountAdjustmentStore Inner { get; set; } = inner;
    public HeadcountAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HeadcountAdjustment? Find(string code)
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

    public void Save(HeadcountAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HeadcountAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects headcount adjustments. Empty fields match all.</summary>
public sealed record HeadcountAdjustmentQuery(string Title, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(HeadcountAdjustment item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HeadcountAdjustment> Apply(List<HeadcountAdjustment> items)
    {
        var matched = Collections.Filter(items, (HeadcountAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of headcount adjustment store.</summary>
public static class HeadcountAdjustmentStore
{
    public static IHeadcountAdjustmentStore NewMemoryHeadcountAdjustmentStore()
    {
        return new MemoryHeadcountAdjustmentStore(new Dictionary<string, HeadcountAdjustment>(), new List<string>());
    }

    public static IHeadcountAdjustmentStore NewCachedHeadcountAdjustmentStore(MemoryHeadcountAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedHeadcountAdjustmentStore(inner, HeadcountAdjustmentCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<HeadcountAdjustment> FindHeadcountAdjustments(IHeadcountAdjustmentStore store, HeadcountAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
