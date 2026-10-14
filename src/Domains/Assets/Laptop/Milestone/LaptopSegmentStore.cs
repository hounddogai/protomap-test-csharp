namespace Acme.Workforce.Assets.Laptop.Milestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps laptop segments by code.</summary>
public interface ILaptopSegmentStore
{
    LaptopSegment? Find(string code);
    void Save(LaptopSegment item);
    bool Remove(string code);
    List<LaptopSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLaptopSegmentStore(Dictionary<string, LaptopSegment> items, List<string> order) : ILaptopSegmentStore
{
    public Dictionary<string, LaptopSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LaptopSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LaptopSegment item)
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

    public List<LaptopSegment> List()
    {
        var outValue = new List<LaptopSegment>();
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
public sealed class CachedLaptopSegmentStore(MemoryLaptopSegmentStore inner, LaptopSegmentCache cache, int hits, int misses) : ILaptopSegmentStore
{
    public MemoryLaptopSegmentStore Inner { get; set; } = inner;
    public LaptopSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LaptopSegment? Find(string code)
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

    public void Save(LaptopSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LaptopSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects laptop segments. Empty fields match all.</summary>
public sealed record LaptopSegmentQuery(string Name, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(LaptopSegment item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LaptopSegment> Apply(List<LaptopSegment> items)
    {
        var matched = Collections.Filter(items, (LaptopSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of laptop segment store.</summary>
public static class LaptopSegmentStore
{
    public static ILaptopSegmentStore NewMemoryLaptopSegmentStore()
    {
        return new MemoryLaptopSegmentStore(new Dictionary<string, LaptopSegment>(), new List<string>());
    }

    public static ILaptopSegmentStore NewCachedLaptopSegmentStore(MemoryLaptopSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedLaptopSegmentStore(inner, LaptopSegmentCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<LaptopSegment> FindLaptopSegments(ILaptopSegmentStore store, LaptopSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
