namespace Acme.Workforce.Assets.LicenseSeat.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps license seat segments by code.</summary>
public interface ILicenseSeatSegmentStore
{
    LicenseSeatSegment? Find(string code);
    void Save(LicenseSeatSegment item);
    bool Remove(string code);
    List<LicenseSeatSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLicenseSeatSegmentStore(Dictionary<string, LicenseSeatSegment> items, List<string> order) : ILicenseSeatSegmentStore
{
    public Dictionary<string, LicenseSeatSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LicenseSeatSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LicenseSeatSegment item)
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

    public List<LicenseSeatSegment> List()
    {
        var outValue = new List<LicenseSeatSegment>();
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
public sealed class CachedLicenseSeatSegmentStore(MemoryLicenseSeatSegmentStore inner, LicenseSeatSegmentCache cache, int hits, int misses) : ILicenseSeatSegmentStore
{
    public MemoryLicenseSeatSegmentStore Inner { get; set; } = inner;
    public LicenseSeatSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LicenseSeatSegment? Find(string code)
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

    public void Save(LicenseSeatSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LicenseSeatSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects license seat segments. Empty fields match all.</summary>
public sealed record LicenseSeatSegmentQuery(string Label, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(LicenseSeatSegment item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LicenseSeatSegment> Apply(List<LicenseSeatSegment> items)
    {
        var matched = Collections.Filter(items, (LicenseSeatSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of license seat segment store.</summary>
public static class LicenseSeatSegmentStore
{
    public static ILicenseSeatSegmentStore NewMemoryLicenseSeatSegmentStore()
    {
        return new MemoryLicenseSeatSegmentStore(new Dictionary<string, LicenseSeatSegment>(), new List<string>());
    }

    public static ILicenseSeatSegmentStore NewCachedLicenseSeatSegmentStore(MemoryLicenseSeatSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedLicenseSeatSegmentStore(inner, LicenseSeatSegmentCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<LicenseSeatSegment> FindLicenseSeatSegments(ILicenseSeatSegmentStore store, LicenseSeatSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
