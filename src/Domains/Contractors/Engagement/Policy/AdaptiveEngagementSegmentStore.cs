namespace Acme.Workforce.Contractors.Engagement.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive engagement segments by code.</summary>
public interface IAdaptiveEngagementSegmentStore
{
    AdaptiveEngagementSegment? Find(string code);
    void Save(AdaptiveEngagementSegment item);
    bool Remove(string code);
    List<AdaptiveEngagementSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveEngagementSegmentStore(Dictionary<string, AdaptiveEngagementSegment> items, List<string> order) : IAdaptiveEngagementSegmentStore
{
    public Dictionary<string, AdaptiveEngagementSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveEngagementSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveEngagementSegment item)
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

    public List<AdaptiveEngagementSegment> List()
    {
        var outValue = new List<AdaptiveEngagementSegment>();
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
public sealed class CachedAdaptiveEngagementSegmentStore(MemoryAdaptiveEngagementSegmentStore inner, AdaptiveEngagementSegmentCache cache, int hits, int misses) : IAdaptiveEngagementSegmentStore
{
    public MemoryAdaptiveEngagementSegmentStore Inner { get; set; } = inner;
    public AdaptiveEngagementSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveEngagementSegment? Find(string code)
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

    public void Save(AdaptiveEngagementSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveEngagementSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive engagement segments. Empty fields match all.</summary>
public sealed record AdaptiveEngagementSegmentQuery(string Unit, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveEngagementSegment item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveEngagementSegment> Apply(List<AdaptiveEngagementSegment> items)
    {
        var matched = Collections.Filter(items, (AdaptiveEngagementSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive engagement segment store.</summary>
public static class AdaptiveEngagementSegmentStore
{
    public static IAdaptiveEngagementSegmentStore NewMemoryAdaptiveEngagementSegmentStore()
    {
        return new MemoryAdaptiveEngagementSegmentStore(
            new Dictionary<string, AdaptiveEngagementSegment>(),
            new List<string>());
    }

    public static IAdaptiveEngagementSegmentStore NewCachedAdaptiveEngagementSegmentStore(MemoryAdaptiveEngagementSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveEngagementSegmentStore(
            inner,
            AdaptiveEngagementSegmentCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveEngagementSegment> FindAdaptiveEngagementSegments(IAdaptiveEngagementSegmentStore store, AdaptiveEngagementSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
