namespace Acme.Workforce.Payrollops.PayRun.CappedRunLedger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay run segments by code.</summary>
public interface IPayRunSegmentStore
{
    PayRunSegment? Find(string code);
    void Save(PayRunSegment item);
    bool Remove(string code);
    List<PayRunSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayRunSegmentStore(Dictionary<string, PayRunSegment> items, List<string> order) : IPayRunSegmentStore
{
    public Dictionary<string, PayRunSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayRunSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayRunSegment item)
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

    public List<PayRunSegment> List()
    {
        var outValue = new List<PayRunSegment>();
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
public sealed class CachedPayRunSegmentStore(MemoryPayRunSegmentStore inner, PayRunSegmentCache cache, int hits, int misses) : IPayRunSegmentStore
{
    public MemoryPayRunSegmentStore Inner { get; set; } = inner;
    public PayRunSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayRunSegment? Find(string code)
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

    public void Save(PayRunSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayRunSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay run segments. Empty fields match all.</summary>
public sealed record PayRunSegmentQuery(string Region, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(PayRunSegment item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayRunSegment> Apply(List<PayRunSegment> items)
    {
        var matched = Collections.Filter(items, (PayRunSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay run segment store.</summary>
public static class PayRunSegmentStore
{
    public static IPayRunSegmentStore NewMemoryPayRunSegmentStore()
    {
        return new MemoryPayRunSegmentStore(new Dictionary<string, PayRunSegment>(), new List<string>());
    }

    public static IPayRunSegmentStore NewCachedPayRunSegmentStore(MemoryPayRunSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedPayRunSegmentStore(inner, PayRunSegmentCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<PayRunSegment> FindPayRunSegments(IPayRunSegmentStore store, PayRunSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
