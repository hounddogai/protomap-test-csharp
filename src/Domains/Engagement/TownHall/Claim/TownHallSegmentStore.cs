namespace Acme.Workforce.Engagement.TownHall.Claim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps town hall segments by code.</summary>
public interface ITownHallSegmentStore
{
    TownHallSegment? Find(string code);
    void Save(TownHallSegment item);
    bool Remove(string code);
    List<TownHallSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTownHallSegmentStore(Dictionary<string, TownHallSegment> items, List<string> order) : ITownHallSegmentStore
{
    public Dictionary<string, TownHallSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TownHallSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TownHallSegment item)
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

    public List<TownHallSegment> List()
    {
        var outValue = new List<TownHallSegment>();
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
public sealed class CachedTownHallSegmentStore(MemoryTownHallSegmentStore inner, TownHallSegmentCache cache, int hits, int misses) : ITownHallSegmentStore
{
    public MemoryTownHallSegmentStore Inner { get; set; } = inner;
    public TownHallSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TownHallSegment? Find(string code)
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

    public void Save(TownHallSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TownHallSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects town hall segments. Empty fields match all.</summary>
public sealed record TownHallSegmentQuery(string Name, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(TownHallSegment item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TownHallSegment> Apply(List<TownHallSegment> items)
    {
        var matched = Collections.Filter(items, (TownHallSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of town hall segment store.</summary>
public static class TownHallSegmentStore
{
    public static ITownHallSegmentStore NewMemoryTownHallSegmentStore()
    {
        return new MemoryTownHallSegmentStore(new Dictionary<string, TownHallSegment>(), new List<string>());
    }

    public static ITownHallSegmentStore NewCachedTownHallSegmentStore(MemoryTownHallSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedTownHallSegmentStore(inner, TownHallSegmentCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<TownHallSegment> FindTownHallSegments(ITownHallSegmentStore store, TownHallSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
