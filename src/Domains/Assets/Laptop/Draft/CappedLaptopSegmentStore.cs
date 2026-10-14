namespace Acme.Workforce.Assets.Laptop.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped laptop segments by code.</summary>
public interface ICappedLaptopSegmentStore
{
    CappedLaptopSegment? Find(string code);
    void Save(CappedLaptopSegment item);
    bool Remove(string code);
    List<CappedLaptopSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedLaptopSegmentStore(Dictionary<string, CappedLaptopSegment> items, List<string> order) : ICappedLaptopSegmentStore
{
    public Dictionary<string, CappedLaptopSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedLaptopSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedLaptopSegment item)
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

    public List<CappedLaptopSegment> List()
    {
        var outValue = new List<CappedLaptopSegment>();
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
public sealed class CachedCappedLaptopSegmentStore(MemoryCappedLaptopSegmentStore inner, CappedLaptopSegmentCache cache, int hits, int misses) : ICappedLaptopSegmentStore
{
    public MemoryCappedLaptopSegmentStore Inner { get; set; } = inner;
    public CappedLaptopSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedLaptopSegment? Find(string code)
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

    public void Save(CappedLaptopSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedLaptopSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped laptop segments. Empty fields match all.</summary>
public sealed record CappedLaptopSegmentQuery(string Channel, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(CappedLaptopSegment item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedLaptopSegment> Apply(List<CappedLaptopSegment> items)
    {
        var matched = Collections.Filter(items, (CappedLaptopSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped laptop segment store.</summary>
public static class CappedLaptopSegmentStore
{
    public static ICappedLaptopSegmentStore NewMemoryCappedLaptopSegmentStore()
    {
        return new MemoryCappedLaptopSegmentStore(new Dictionary<string, CappedLaptopSegment>(), new List<string>());
    }

    public static ICappedLaptopSegmentStore NewCachedCappedLaptopSegmentStore(MemoryCappedLaptopSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedCappedLaptopSegmentStore(inner, CappedLaptopSegmentCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<CappedLaptopSegment> FindCappedLaptopSegments(ICappedLaptopSegmentStore store, CappedLaptopSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
