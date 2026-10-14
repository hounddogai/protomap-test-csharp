namespace Acme.Workforce.Contractors.Engagement.EngagementSnapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict engagement snapshots by code.</summary>
public interface IStrictEngagementSnapshotStore
{
    StrictEngagementSnapshot? Find(string code);
    void Save(StrictEngagementSnapshot item);
    bool Remove(string code);
    List<StrictEngagementSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictEngagementSnapshotStore(Dictionary<string, StrictEngagementSnapshot> items, List<string> order) : IStrictEngagementSnapshotStore
{
    public Dictionary<string, StrictEngagementSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictEngagementSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictEngagementSnapshot item)
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

    public List<StrictEngagementSnapshot> List()
    {
        var outValue = new List<StrictEngagementSnapshot>();
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
public sealed class CachedStrictEngagementSnapshotStore(MemoryStrictEngagementSnapshotStore inner, StrictEngagementSnapshotCache cache, int hits, int misses) : IStrictEngagementSnapshotStore
{
    public MemoryStrictEngagementSnapshotStore Inner { get; set; } = inner;
    public StrictEngagementSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictEngagementSnapshot? Find(string code)
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

    public void Save(StrictEngagementSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictEngagementSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict engagement snapshots. Empty fields match all.</summary>
public sealed record StrictEngagementSnapshotQuery(string Country, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(StrictEngagementSnapshot item)
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

    public List<StrictEngagementSnapshot> Apply(List<StrictEngagementSnapshot> items)
    {
        var matched = Collections.Filter(items, (StrictEngagementSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict engagement snapshot store.</summary>
public static class StrictEngagementSnapshotStore
{
    public static IStrictEngagementSnapshotStore NewMemoryStrictEngagementSnapshotStore()
    {
        return new MemoryStrictEngagementSnapshotStore(
            new Dictionary<string, StrictEngagementSnapshot>(),
            new List<string>());
    }

    public static IStrictEngagementSnapshotStore NewCachedStrictEngagementSnapshotStore(MemoryStrictEngagementSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedStrictEngagementSnapshotStore(
            inner,
            StrictEngagementSnapshotCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<StrictEngagementSnapshot> FindStrictEngagementSnapshots(IStrictEngagementSnapshotStore store, StrictEngagementSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
