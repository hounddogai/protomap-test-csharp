namespace Acme.Workforce.Payrollops.Deduction.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict deduction segments by code.</summary>
public interface IStrictDeductionSegmentStore
{
    StrictDeductionSegment? Find(string code);
    void Save(StrictDeductionSegment item);
    bool Remove(string code);
    List<StrictDeductionSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictDeductionSegmentStore(Dictionary<string, StrictDeductionSegment> items, List<string> order) : IStrictDeductionSegmentStore
{
    public Dictionary<string, StrictDeductionSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictDeductionSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictDeductionSegment item)
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

    public List<StrictDeductionSegment> List()
    {
        var outValue = new List<StrictDeductionSegment>();
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
public sealed class CachedStrictDeductionSegmentStore(MemoryStrictDeductionSegmentStore inner, StrictDeductionSegmentCache cache, int hits, int misses) : IStrictDeductionSegmentStore
{
    public MemoryStrictDeductionSegmentStore Inner { get; set; } = inner;
    public StrictDeductionSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictDeductionSegment? Find(string code)
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

    public void Save(StrictDeductionSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictDeductionSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict deduction segments. Empty fields match all.</summary>
public sealed record StrictDeductionSegmentQuery(string Name, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(StrictDeductionSegment item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictDeductionSegment> Apply(List<StrictDeductionSegment> items)
    {
        var matched = Collections.Filter(items, (StrictDeductionSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict deduction segment store.</summary>
public static class StrictDeductionSegmentStore
{
    public static IStrictDeductionSegmentStore NewMemoryStrictDeductionSegmentStore()
    {
        return new MemoryStrictDeductionSegmentStore(
            new Dictionary<string, StrictDeductionSegment>(),
            new List<string>());
    }

    public static IStrictDeductionSegmentStore NewCachedStrictDeductionSegmentStore(MemoryStrictDeductionSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedStrictDeductionSegmentStore(
            inner,
            StrictDeductionSegmentCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<StrictDeductionSegment> FindStrictDeductionSegments(IStrictDeductionSegmentStore store, StrictDeductionSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
