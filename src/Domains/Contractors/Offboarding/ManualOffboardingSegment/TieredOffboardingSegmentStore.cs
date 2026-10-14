namespace Acme.Workforce.Contractors.Offboarding.ManualOffboardingSegment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered offboarding segments by code.</summary>
public interface ITieredOffboardingSegmentStore
{
    TieredOffboardingSegment? Find(string code);
    void Save(TieredOffboardingSegment item);
    bool Remove(string code);
    List<TieredOffboardingSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredOffboardingSegmentStore(Dictionary<string, TieredOffboardingSegment> items, List<string> order) : ITieredOffboardingSegmentStore
{
    public Dictionary<string, TieredOffboardingSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredOffboardingSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredOffboardingSegment item)
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

    public List<TieredOffboardingSegment> List()
    {
        var outValue = new List<TieredOffboardingSegment>();
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
public sealed class CachedTieredOffboardingSegmentStore(MemoryTieredOffboardingSegmentStore inner, TieredOffboardingSegmentCache cache, int hits, int misses) : ITieredOffboardingSegmentStore
{
    public MemoryTieredOffboardingSegmentStore Inner { get; set; } = inner;
    public TieredOffboardingSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredOffboardingSegment? Find(string code)
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

    public void Save(TieredOffboardingSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredOffboardingSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered offboarding segments. Empty fields match all.</summary>
public sealed record TieredOffboardingSegmentQuery(string Title, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(TieredOffboardingSegment item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<TieredOffboardingSegment> Apply(List<TieredOffboardingSegment> items)
    {
        var matched = Collections.Filter(items, (TieredOffboardingSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered offboarding segment store.</summary>
public static class TieredOffboardingSegmentStore
{
    public static ITieredOffboardingSegmentStore NewMemoryTieredOffboardingSegmentStore()
    {
        return new MemoryTieredOffboardingSegmentStore(
            new Dictionary<string, TieredOffboardingSegment>(),
            new List<string>());
    }

    public static ITieredOffboardingSegmentStore NewCachedTieredOffboardingSegmentStore(MemoryTieredOffboardingSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedTieredOffboardingSegmentStore(
            inner,
            TieredOffboardingSegmentCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<TieredOffboardingSegment> FindTieredOffboardingSegments(ITieredOffboardingSegmentStore store, TieredOffboardingSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
