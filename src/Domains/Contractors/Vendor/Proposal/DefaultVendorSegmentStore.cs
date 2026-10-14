namespace Acme.Workforce.Contractors.Vendor.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default vendor segments by code.</summary>
public interface IDefaultVendorSegmentStore
{
    DefaultVendorSegment? Find(string code);
    void Save(DefaultVendorSegment item);
    bool Remove(string code);
    List<DefaultVendorSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultVendorSegmentStore(Dictionary<string, DefaultVendorSegment> items, List<string> order) : IDefaultVendorSegmentStore
{
    public Dictionary<string, DefaultVendorSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultVendorSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultVendorSegment item)
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

    public List<DefaultVendorSegment> List()
    {
        var outValue = new List<DefaultVendorSegment>();
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
public sealed class CachedDefaultVendorSegmentStore(MemoryDefaultVendorSegmentStore inner, DefaultVendorSegmentCache cache, int hits, int misses) : IDefaultVendorSegmentStore
{
    public MemoryDefaultVendorSegmentStore Inner { get; set; } = inner;
    public DefaultVendorSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultVendorSegment? Find(string code)
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

    public void Save(DefaultVendorSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultVendorSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default vendor segments. Empty fields match all.</summary>
public sealed record DefaultVendorSegmentQuery(string Segment, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultVendorSegment item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
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

    public List<DefaultVendorSegment> Apply(List<DefaultVendorSegment> items)
    {
        var matched = Collections.Filter(items, (DefaultVendorSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default vendor segment store.</summary>
public static class DefaultVendorSegmentStore
{
    public static IDefaultVendorSegmentStore NewMemoryDefaultVendorSegmentStore()
    {
        return new MemoryDefaultVendorSegmentStore(
            new Dictionary<string, DefaultVendorSegment>(),
            new List<string>());
    }

    public static IDefaultVendorSegmentStore NewCachedDefaultVendorSegmentStore(MemoryDefaultVendorSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultVendorSegmentStore(inner, DefaultVendorSegmentCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<DefaultVendorSegment> FindDefaultVendorSegments(IDefaultVendorSegmentStore store, DefaultVendorSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
