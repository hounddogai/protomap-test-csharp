namespace Acme.Workforce.Contractors.StatementOfWork.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback statement of work segments by code.</summary>
public interface IFallbackStatementOfWorkSegmentStore
{
    FallbackStatementOfWorkSegment? Find(string code);
    void Save(FallbackStatementOfWorkSegment item);
    bool Remove(string code);
    List<FallbackStatementOfWorkSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackStatementOfWorkSegmentStore(Dictionary<string, FallbackStatementOfWorkSegment> items, List<string> order) : IFallbackStatementOfWorkSegmentStore
{
    public Dictionary<string, FallbackStatementOfWorkSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackStatementOfWorkSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackStatementOfWorkSegment item)
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

    public List<FallbackStatementOfWorkSegment> List()
    {
        var outValue = new List<FallbackStatementOfWorkSegment>();
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
public sealed class CachedFallbackStatementOfWorkSegmentStore(MemoryFallbackStatementOfWorkSegmentStore inner, FallbackStatementOfWorkSegmentCache cache, int hits, int misses) : IFallbackStatementOfWorkSegmentStore
{
    public MemoryFallbackStatementOfWorkSegmentStore Inner { get; set; } = inner;
    public FallbackStatementOfWorkSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackStatementOfWorkSegment? Find(string code)
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

    public void Save(FallbackStatementOfWorkSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackStatementOfWorkSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback statement of work segments. Empty fields match all.</summary>
public sealed record FallbackStatementOfWorkSegmentQuery(string Name, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackStatementOfWorkSegment item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<FallbackStatementOfWorkSegment> Apply(List<FallbackStatementOfWorkSegment> items)
    {
        var matched = Collections.Filter(items, (FallbackStatementOfWorkSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback statement of work segment store.</summary>
public static class FallbackStatementOfWorkSegmentStore
{
    public static IFallbackStatementOfWorkSegmentStore NewMemoryFallbackStatementOfWorkSegmentStore()
    {
        return new MemoryFallbackStatementOfWorkSegmentStore(
            new Dictionary<string, FallbackStatementOfWorkSegment>(),
            new List<string>());
    }

    public static IFallbackStatementOfWorkSegmentStore NewCachedFallbackStatementOfWorkSegmentStore(MemoryFallbackStatementOfWorkSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackStatementOfWorkSegmentStore(
            inner,
            FallbackStatementOfWorkSegmentCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<FallbackStatementOfWorkSegment> FindFallbackStatementOfWorkSegments(IFallbackStatementOfWorkSegmentStore store, FallbackStatementOfWorkSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
