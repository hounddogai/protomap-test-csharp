namespace Acme.Workforce.Absence.Balance.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual balance segments by code.</summary>
public interface IManualBalanceSegmentStore
{
    ManualBalanceSegment? Find(string code);
    void Save(ManualBalanceSegment item);
    bool Remove(string code);
    List<ManualBalanceSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualBalanceSegmentStore(Dictionary<string, ManualBalanceSegment> items, List<string> order) : IManualBalanceSegmentStore
{
    public Dictionary<string, ManualBalanceSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualBalanceSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualBalanceSegment item)
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

    public List<ManualBalanceSegment> List()
    {
        var outValue = new List<ManualBalanceSegment>();
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
public sealed class CachedManualBalanceSegmentStore(MemoryManualBalanceSegmentStore inner, ManualBalanceSegmentCache cache, int hits, int misses) : IManualBalanceSegmentStore
{
    public MemoryManualBalanceSegmentStore Inner { get; set; } = inner;
    public ManualBalanceSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualBalanceSegment? Find(string code)
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

    public void Save(ManualBalanceSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualBalanceSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual balance segments. Empty fields match all.</summary>
public sealed record ManualBalanceSegmentQuery(string Country, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(ManualBalanceSegment item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualBalanceSegment> Apply(List<ManualBalanceSegment> items)
    {
        var matched = Collections.Filter(items, (ManualBalanceSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual balance segment store.</summary>
public static class ManualBalanceSegmentStore
{
    public static IManualBalanceSegmentStore NewMemoryManualBalanceSegmentStore()
    {
        return new MemoryManualBalanceSegmentStore(
            new Dictionary<string, ManualBalanceSegment>(),
            new List<string>());
    }

    public static IManualBalanceSegmentStore NewCachedManualBalanceSegmentStore(MemoryManualBalanceSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedManualBalanceSegmentStore(inner, ManualBalanceSegmentCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<ManualBalanceSegment> FindManualBalanceSegments(IManualBalanceSegmentStore store, ManualBalanceSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
