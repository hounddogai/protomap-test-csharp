namespace Acme.Workforce.Absence.ReturnPlan.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps return plan segments by code.</summary>
public interface IReturnPlanSegmentStore
{
    ReturnPlanSegment? Find(string code);
    void Save(ReturnPlanSegment item);
    bool Remove(string code);
    List<ReturnPlanSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReturnPlanSegmentStore(Dictionary<string, ReturnPlanSegment> items, List<string> order) : IReturnPlanSegmentStore
{
    public Dictionary<string, ReturnPlanSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReturnPlanSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReturnPlanSegment item)
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

    public List<ReturnPlanSegment> List()
    {
        var outValue = new List<ReturnPlanSegment>();
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
public sealed class CachedReturnPlanSegmentStore(MemoryReturnPlanSegmentStore inner, ReturnPlanSegmentCache cache, int hits, int misses) : IReturnPlanSegmentStore
{
    public MemoryReturnPlanSegmentStore Inner { get; set; } = inner;
    public ReturnPlanSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReturnPlanSegment? Find(string code)
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

    public void Save(ReturnPlanSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReturnPlanSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects return plan segments. Empty fields match all.</summary>
public sealed record ReturnPlanSegmentQuery(string Segment, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(ReturnPlanSegment item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ReturnPlanSegment> Apply(List<ReturnPlanSegment> items)
    {
        var matched = Collections.Filter(items, (ReturnPlanSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of return plan segment store.</summary>
public static class ReturnPlanSegmentStore
{
    public static IReturnPlanSegmentStore NewMemoryReturnPlanSegmentStore()
    {
        return new MemoryReturnPlanSegmentStore(new Dictionary<string, ReturnPlanSegment>(), new List<string>());
    }

    public static IReturnPlanSegmentStore NewCachedReturnPlanSegmentStore(MemoryReturnPlanSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedReturnPlanSegmentStore(inner, ReturnPlanSegmentCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<ReturnPlanSegment> FindReturnPlanSegments(IReturnPlanSegmentStore store, ReturnPlanSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
