namespace Acme.Workforce.Compensation.MeritCycle.CycleSegment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted merit cycle segments by code.</summary>
public interface IWeightedMeritCycleSegmentStore
{
    WeightedMeritCycleSegment? Find(string code);
    void Save(WeightedMeritCycleSegment item);
    bool Remove(string code);
    List<WeightedMeritCycleSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedMeritCycleSegmentStore(Dictionary<string, WeightedMeritCycleSegment> items, List<string> order) : IWeightedMeritCycleSegmentStore
{
    public Dictionary<string, WeightedMeritCycleSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedMeritCycleSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedMeritCycleSegment item)
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

    public List<WeightedMeritCycleSegment> List()
    {
        var outValue = new List<WeightedMeritCycleSegment>();
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
public sealed class CachedWeightedMeritCycleSegmentStore(MemoryWeightedMeritCycleSegmentStore inner, WeightedMeritCycleSegmentCache cache, int hits, int misses) : IWeightedMeritCycleSegmentStore
{
    public MemoryWeightedMeritCycleSegmentStore Inner { get; set; } = inner;
    public WeightedMeritCycleSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedMeritCycleSegment? Find(string code)
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

    public void Save(WeightedMeritCycleSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedMeritCycleSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted merit cycle segments. Empty fields match all.</summary>
public sealed record WeightedMeritCycleSegmentQuery(string Region, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedMeritCycleSegment item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
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

    public List<WeightedMeritCycleSegment> Apply(List<WeightedMeritCycleSegment> items)
    {
        var matched = Collections.Filter(items, (WeightedMeritCycleSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted merit cycle segment store.</summary>
public static class WeightedMeritCycleSegmentStore
{
    public static IWeightedMeritCycleSegmentStore NewMemoryWeightedMeritCycleSegmentStore()
    {
        return new MemoryWeightedMeritCycleSegmentStore(
            new Dictionary<string, WeightedMeritCycleSegment>(),
            new List<string>());
    }

    public static IWeightedMeritCycleSegmentStore NewCachedWeightedMeritCycleSegmentStore(MemoryWeightedMeritCycleSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedMeritCycleSegmentStore(
            inner,
            WeightedMeritCycleSegmentCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<WeightedMeritCycleSegment> FindWeightedMeritCycleSegments(IWeightedMeritCycleSegmentStore store, WeightedMeritCycleSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
