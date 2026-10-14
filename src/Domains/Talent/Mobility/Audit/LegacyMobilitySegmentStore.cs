namespace Acme.Workforce.Talent.Mobility.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy mobility segments by code.</summary>
public interface ILegacyMobilitySegmentStore
{
    LegacyMobilitySegment? Find(string code);
    void Save(LegacyMobilitySegment item);
    bool Remove(string code);
    List<LegacyMobilitySegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyMobilitySegmentStore(Dictionary<string, LegacyMobilitySegment> items, List<string> order) : ILegacyMobilitySegmentStore
{
    public Dictionary<string, LegacyMobilitySegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyMobilitySegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyMobilitySegment item)
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

    public List<LegacyMobilitySegment> List()
    {
        var outValue = new List<LegacyMobilitySegment>();
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
public sealed class CachedLegacyMobilitySegmentStore(MemoryLegacyMobilitySegmentStore inner, LegacyMobilitySegmentCache cache, int hits, int misses) : ILegacyMobilitySegmentStore
{
    public MemoryLegacyMobilitySegmentStore Inner { get; set; } = inner;
    public LegacyMobilitySegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyMobilitySegment? Find(string code)
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

    public void Save(LegacyMobilitySegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyMobilitySegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy mobility segments. Empty fields match all.</summary>
public sealed record LegacyMobilitySegmentQuery(string Department, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyMobilitySegment item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyMobilitySegment> Apply(List<LegacyMobilitySegment> items)
    {
        var matched = Collections.Filter(items, (LegacyMobilitySegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy mobility segment store.</summary>
public static class LegacyMobilitySegmentStore
{
    public static ILegacyMobilitySegmentStore NewMemoryLegacyMobilitySegmentStore()
    {
        return new MemoryLegacyMobilitySegmentStore(
            new Dictionary<string, LegacyMobilitySegment>(),
            new List<string>());
    }

    public static ILegacyMobilitySegmentStore NewCachedLegacyMobilitySegmentStore(MemoryLegacyMobilitySegmentStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyMobilitySegmentStore(inner, LegacyMobilitySegmentCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<LegacyMobilitySegment> FindLegacyMobilitySegments(ILegacyMobilitySegmentStore store, LegacyMobilitySegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
