namespace Acme.Workforce.Compensation.BonusPool.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps bonus pool segments by code.</summary>
public interface IBonusPoolSegmentStore
{
    BonusPoolSegment? Find(string code);
    void Save(BonusPoolSegment item);
    bool Remove(string code);
    List<BonusPoolSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBonusPoolSegmentStore(Dictionary<string, BonusPoolSegment> items, List<string> order) : IBonusPoolSegmentStore
{
    public Dictionary<string, BonusPoolSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BonusPoolSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BonusPoolSegment item)
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

    public List<BonusPoolSegment> List()
    {
        var outValue = new List<BonusPoolSegment>();
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
public sealed class CachedBonusPoolSegmentStore(MemoryBonusPoolSegmentStore inner, BonusPoolSegmentCache cache, int hits, int misses) : IBonusPoolSegmentStore
{
    public MemoryBonusPoolSegmentStore Inner { get; set; } = inner;
    public BonusPoolSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BonusPoolSegment? Find(string code)
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

    public void Save(BonusPoolSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BonusPoolSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects bonus pool segments. Empty fields match all.</summary>
public sealed record BonusPoolSegmentQuery(string Country, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(BonusPoolSegment item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BonusPoolSegment> Apply(List<BonusPoolSegment> items)
    {
        var matched = Collections.Filter(items, (BonusPoolSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of bonus pool segment store.</summary>
public static class BonusPoolSegmentStore
{
    public static IBonusPoolSegmentStore NewMemoryBonusPoolSegmentStore()
    {
        return new MemoryBonusPoolSegmentStore(new Dictionary<string, BonusPoolSegment>(), new List<string>());
    }

    public static IBonusPoolSegmentStore NewCachedBonusPoolSegmentStore(MemoryBonusPoolSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedBonusPoolSegmentStore(inner, BonusPoolSegmentCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<BonusPoolSegment> FindBonusPoolSegments(IBonusPoolSegmentStore store, BonusPoolSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
