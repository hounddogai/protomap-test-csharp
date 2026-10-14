namespace Acme.Workforce.Contractors.Engagement.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps engagement segments by code.</summary>
public interface IEngagementSegmentStore
{
    EngagementSegment? Find(string code);
    void Save(EngagementSegment item);
    bool Remove(string code);
    List<EngagementSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryEngagementSegmentStore(Dictionary<string, EngagementSegment> items, List<string> order) : IEngagementSegmentStore
{
    public Dictionary<string, EngagementSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public EngagementSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(EngagementSegment item)
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

    public List<EngagementSegment> List()
    {
        var outValue = new List<EngagementSegment>();
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
public sealed class CachedEngagementSegmentStore(MemoryEngagementSegmentStore inner, EngagementSegmentCache cache, int hits, int misses) : IEngagementSegmentStore
{
    public MemoryEngagementSegmentStore Inner { get; set; } = inner;
    public EngagementSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public EngagementSegment? Find(string code)
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

    public void Save(EngagementSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<EngagementSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects engagement segments. Empty fields match all.</summary>
public sealed record EngagementSegmentQuery(string Channel, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(EngagementSegment item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<EngagementSegment> Apply(List<EngagementSegment> items)
    {
        var matched = Collections.Filter(items, (EngagementSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of engagement segment store.</summary>
public static class EngagementSegmentStore
{
    public static IEngagementSegmentStore NewMemoryEngagementSegmentStore()
    {
        return new MemoryEngagementSegmentStore(new Dictionary<string, EngagementSegment>(), new List<string>());
    }

    public static IEngagementSegmentStore NewCachedEngagementSegmentStore(MemoryEngagementSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedEngagementSegmentStore(inner, EngagementSegmentCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<EngagementSegment> FindEngagementSegments(IEngagementSegmentStore store, EngagementSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
