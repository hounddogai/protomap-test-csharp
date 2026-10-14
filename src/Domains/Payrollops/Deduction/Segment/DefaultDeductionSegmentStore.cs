namespace Acme.Workforce.Payrollops.Deduction.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default deduction segments by code.</summary>
public interface IDefaultDeductionSegmentStore
{
    DefaultDeductionSegment? Find(string code);
    void Save(DefaultDeductionSegment item);
    bool Remove(string code);
    List<DefaultDeductionSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultDeductionSegmentStore(Dictionary<string, DefaultDeductionSegment> items, List<string> order) : IDefaultDeductionSegmentStore
{
    public Dictionary<string, DefaultDeductionSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultDeductionSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultDeductionSegment item)
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

    public List<DefaultDeductionSegment> List()
    {
        var outValue = new List<DefaultDeductionSegment>();
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
public sealed class CachedDefaultDeductionSegmentStore(MemoryDefaultDeductionSegmentStore inner, DefaultDeductionSegmentCache cache, int hits, int misses) : IDefaultDeductionSegmentStore
{
    public MemoryDefaultDeductionSegmentStore Inner { get; set; } = inner;
    public DefaultDeductionSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultDeductionSegment? Find(string code)
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

    public void Save(DefaultDeductionSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultDeductionSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default deduction segments. Empty fields match all.</summary>
public sealed record DefaultDeductionSegmentQuery(string Source, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultDeductionSegment item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultDeductionSegment> Apply(List<DefaultDeductionSegment> items)
    {
        var matched = Collections.Filter(items, (DefaultDeductionSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default deduction segment store.</summary>
public static class DefaultDeductionSegmentStore
{
    public static IDefaultDeductionSegmentStore NewMemoryDefaultDeductionSegmentStore()
    {
        return new MemoryDefaultDeductionSegmentStore(
            new Dictionary<string, DefaultDeductionSegment>(),
            new List<string>());
    }

    public static IDefaultDeductionSegmentStore NewCachedDefaultDeductionSegmentStore(MemoryDefaultDeductionSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultDeductionSegmentStore(
            inner,
            DefaultDeductionSegmentCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<DefaultDeductionSegment> FindDefaultDeductionSegments(IDefaultDeductionSegmentStore store, DefaultDeductionSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
