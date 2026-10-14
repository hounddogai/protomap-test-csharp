namespace Acme.Workforce.Contractors.Offboarding.OffboardingSegment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive offboarding segments by code.</summary>
public interface IAdaptiveOffboardingSegmentStore
{
    AdaptiveOffboardingSegment? Find(string code);
    void Save(AdaptiveOffboardingSegment item);
    bool Remove(string code);
    List<AdaptiveOffboardingSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveOffboardingSegmentStore(Dictionary<string, AdaptiveOffboardingSegment> items, List<string> order) : IAdaptiveOffboardingSegmentStore
{
    public Dictionary<string, AdaptiveOffboardingSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveOffboardingSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveOffboardingSegment item)
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

    public List<AdaptiveOffboardingSegment> List()
    {
        var outValue = new List<AdaptiveOffboardingSegment>();
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
public sealed class CachedAdaptiveOffboardingSegmentStore(MemoryAdaptiveOffboardingSegmentStore inner, AdaptiveOffboardingSegmentCache cache, int hits, int misses) : IAdaptiveOffboardingSegmentStore
{
    public MemoryAdaptiveOffboardingSegmentStore Inner { get; set; } = inner;
    public AdaptiveOffboardingSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveOffboardingSegment? Find(string code)
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

    public void Save(AdaptiveOffboardingSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveOffboardingSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive offboarding segments. Empty fields match all.</summary>
public sealed record AdaptiveOffboardingSegmentQuery(string Title, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveOffboardingSegment item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveOffboardingSegment> Apply(List<AdaptiveOffboardingSegment> items)
    {
        var matched = Collections.Filter(items, (AdaptiveOffboardingSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive offboarding segment store.</summary>
public static class AdaptiveOffboardingSegmentStore
{
    public static IAdaptiveOffboardingSegmentStore NewMemoryAdaptiveOffboardingSegmentStore()
    {
        return new MemoryAdaptiveOffboardingSegmentStore(
            new Dictionary<string, AdaptiveOffboardingSegment>(),
            new List<string>());
    }

    public static IAdaptiveOffboardingSegmentStore NewCachedAdaptiveOffboardingSegmentStore(MemoryAdaptiveOffboardingSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveOffboardingSegmentStore(
            inner,
            AdaptiveOffboardingSegmentCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveOffboardingSegment> FindAdaptiveOffboardingSegments(IAdaptiveOffboardingSegmentStore store, AdaptiveOffboardingSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
