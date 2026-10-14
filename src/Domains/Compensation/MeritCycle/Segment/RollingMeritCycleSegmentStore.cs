namespace Acme.Workforce.Compensation.MeritCycle.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling merit cycle segments by code.</summary>
public interface IRollingMeritCycleSegmentStore
{
    RollingMeritCycleSegment? Find(string code);
    void Save(RollingMeritCycleSegment item);
    bool Remove(string code);
    List<RollingMeritCycleSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingMeritCycleSegmentStore(Dictionary<string, RollingMeritCycleSegment> items, List<string> order) : IRollingMeritCycleSegmentStore
{
    public Dictionary<string, RollingMeritCycleSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingMeritCycleSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingMeritCycleSegment item)
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

    public List<RollingMeritCycleSegment> List()
    {
        var outValue = new List<RollingMeritCycleSegment>();
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
public sealed class CachedRollingMeritCycleSegmentStore(MemoryRollingMeritCycleSegmentStore inner, RollingMeritCycleSegmentCache cache, int hits, int misses) : IRollingMeritCycleSegmentStore
{
    public MemoryRollingMeritCycleSegmentStore Inner { get; set; } = inner;
    public RollingMeritCycleSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingMeritCycleSegment? Find(string code)
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

    public void Save(RollingMeritCycleSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingMeritCycleSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling merit cycle segments. Empty fields match all.</summary>
public sealed record RollingMeritCycleSegmentQuery(string Label, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(RollingMeritCycleSegment item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingMeritCycleSegment> Apply(List<RollingMeritCycleSegment> items)
    {
        var matched = Collections.Filter(items, (RollingMeritCycleSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling merit cycle segment store.</summary>
public static class RollingMeritCycleSegmentStore
{
    public static IRollingMeritCycleSegmentStore NewMemoryRollingMeritCycleSegmentStore()
    {
        return new MemoryRollingMeritCycleSegmentStore(
            new Dictionary<string, RollingMeritCycleSegment>(),
            new List<string>());
    }

    public static IRollingMeritCycleSegmentStore NewCachedRollingMeritCycleSegmentStore(MemoryRollingMeritCycleSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedRollingMeritCycleSegmentStore(
            inner,
            RollingMeritCycleSegmentCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<RollingMeritCycleSegment> FindRollingMeritCycleSegments(IRollingMeritCycleSegmentStore store, RollingMeritCycleSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
