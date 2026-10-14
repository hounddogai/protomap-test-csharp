namespace Acme.Workforce.Contractors.Offboarding.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps offboarding segments by code.</summary>
public interface IOffboardingSegmentStore
{
    OffboardingSegment? Find(string code);
    void Save(OffboardingSegment item);
    bool Remove(string code);
    List<OffboardingSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOffboardingSegmentStore(Dictionary<string, OffboardingSegment> items, List<string> order) : IOffboardingSegmentStore
{
    public Dictionary<string, OffboardingSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OffboardingSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OffboardingSegment item)
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

    public List<OffboardingSegment> List()
    {
        var outValue = new List<OffboardingSegment>();
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
public sealed class CachedOffboardingSegmentStore(MemoryOffboardingSegmentStore inner, OffboardingSegmentCache cache, int hits, int misses) : IOffboardingSegmentStore
{
    public MemoryOffboardingSegmentStore Inner { get; set; } = inner;
    public OffboardingSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OffboardingSegment? Find(string code)
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

    public void Save(OffboardingSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OffboardingSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects offboarding segments. Empty fields match all.</summary>
public sealed record OffboardingSegmentQuery(string Name, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(OffboardingSegment item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<OffboardingSegment> Apply(List<OffboardingSegment> items)
    {
        var matched = Collections.Filter(items, (OffboardingSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of offboarding segment store.</summary>
public static class OffboardingSegmentStore
{
    public static IOffboardingSegmentStore NewMemoryOffboardingSegmentStore()
    {
        return new MemoryOffboardingSegmentStore(new Dictionary<string, OffboardingSegment>(), new List<string>());
    }

    public static IOffboardingSegmentStore NewCachedOffboardingSegmentStore(MemoryOffboardingSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedOffboardingSegmentStore(inner, OffboardingSegmentCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<OffboardingSegment> FindOffboardingSegments(IOffboardingSegmentStore store, OffboardingSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
