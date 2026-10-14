namespace Acme.Workforce.Talent.Pool.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pool segments by code.</summary>
public interface IPoolSegmentStore
{
    PoolSegment? Find(string code);
    void Save(PoolSegment item);
    bool Remove(string code);
    List<PoolSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPoolSegmentStore(Dictionary<string, PoolSegment> items, List<string> order) : IPoolSegmentStore
{
    public Dictionary<string, PoolSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PoolSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PoolSegment item)
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

    public List<PoolSegment> List()
    {
        var outValue = new List<PoolSegment>();
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
public sealed class CachedPoolSegmentStore(MemoryPoolSegmentStore inner, PoolSegmentCache cache, int hits, int misses) : IPoolSegmentStore
{
    public MemoryPoolSegmentStore Inner { get; set; } = inner;
    public PoolSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PoolSegment? Find(string code)
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

    public void Save(PoolSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PoolSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pool segments. Empty fields match all.</summary>
public sealed record PoolSegmentQuery(string Title, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(PoolSegment item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PoolSegment> Apply(List<PoolSegment> items)
    {
        var matched = Collections.Filter(items, (PoolSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pool segment store.</summary>
public static class PoolSegmentStore
{
    public static IPoolSegmentStore NewMemoryPoolSegmentStore()
    {
        return new MemoryPoolSegmentStore(new Dictionary<string, PoolSegment>(), new List<string>());
    }

    public static IPoolSegmentStore NewCachedPoolSegmentStore(MemoryPoolSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedPoolSegmentStore(inner, PoolSegmentCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<PoolSegment> FindPoolSegments(IPoolSegmentStore store, PoolSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
