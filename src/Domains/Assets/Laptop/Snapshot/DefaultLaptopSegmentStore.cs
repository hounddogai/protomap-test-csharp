namespace Acme.Workforce.Assets.Laptop.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default laptop segments by code.</summary>
public interface IDefaultLaptopSegmentStore
{
    DefaultLaptopSegment? Find(string code);
    void Save(DefaultLaptopSegment item);
    bool Remove(string code);
    List<DefaultLaptopSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultLaptopSegmentStore(Dictionary<string, DefaultLaptopSegment> items, List<string> order) : IDefaultLaptopSegmentStore
{
    public Dictionary<string, DefaultLaptopSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultLaptopSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultLaptopSegment item)
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

    public List<DefaultLaptopSegment> List()
    {
        var outValue = new List<DefaultLaptopSegment>();
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
public sealed class CachedDefaultLaptopSegmentStore(MemoryDefaultLaptopSegmentStore inner, DefaultLaptopSegmentCache cache, int hits, int misses) : IDefaultLaptopSegmentStore
{
    public MemoryDefaultLaptopSegmentStore Inner { get; set; } = inner;
    public DefaultLaptopSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultLaptopSegment? Find(string code)
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

    public void Save(DefaultLaptopSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultLaptopSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default laptop segments. Empty fields match all.</summary>
public sealed record DefaultLaptopSegmentQuery(string Label, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultLaptopSegment item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<DefaultLaptopSegment> Apply(List<DefaultLaptopSegment> items)
    {
        var matched = Collections.Filter(items, (DefaultLaptopSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default laptop segment store.</summary>
public static class DefaultLaptopSegmentStore
{
    public static IDefaultLaptopSegmentStore NewMemoryDefaultLaptopSegmentStore()
    {
        return new MemoryDefaultLaptopSegmentStore(
            new Dictionary<string, DefaultLaptopSegment>(),
            new List<string>());
    }

    public static IDefaultLaptopSegmentStore NewCachedDefaultLaptopSegmentStore(MemoryDefaultLaptopSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultLaptopSegmentStore(inner, DefaultLaptopSegmentCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<DefaultLaptopSegment> FindDefaultLaptopSegments(IDefaultLaptopSegmentStore store, DefaultLaptopSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
