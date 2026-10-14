namespace Acme.Workforce.Absence.Accrual.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps accrual segments by code.</summary>
public interface IAccrualSegmentStore
{
    AccrualSegment? Find(string code);
    void Save(AccrualSegment item);
    bool Remove(string code);
    List<AccrualSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccrualSegmentStore(Dictionary<string, AccrualSegment> items, List<string> order) : IAccrualSegmentStore
{
    public Dictionary<string, AccrualSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccrualSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccrualSegment item)
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

    public List<AccrualSegment> List()
    {
        var outValue = new List<AccrualSegment>();
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
public sealed class CachedAccrualSegmentStore(MemoryAccrualSegmentStore inner, AccrualSegmentCache cache, int hits, int misses) : IAccrualSegmentStore
{
    public MemoryAccrualSegmentStore Inner { get; set; } = inner;
    public AccrualSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccrualSegment? Find(string code)
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

    public void Save(AccrualSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccrualSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects accrual segments. Empty fields match all.</summary>
public sealed record AccrualSegmentQuery(string Source, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(AccrualSegment item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AccrualSegment> Apply(List<AccrualSegment> items)
    {
        var matched = Collections.Filter(items, (AccrualSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of accrual segment store.</summary>
public static class AccrualSegmentStore
{
    public static IAccrualSegmentStore NewMemoryAccrualSegmentStore()
    {
        return new MemoryAccrualSegmentStore(new Dictionary<string, AccrualSegment>(), new List<string>());
    }

    public static IAccrualSegmentStore NewCachedAccrualSegmentStore(MemoryAccrualSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedAccrualSegmentStore(inner, AccrualSegmentCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<AccrualSegment> FindAccrualSegments(IAccrualSegmentStore store, AccrualSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
