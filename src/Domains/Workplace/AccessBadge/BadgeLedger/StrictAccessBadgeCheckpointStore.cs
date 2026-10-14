namespace Acme.Workforce.Workplace.AccessBadge.BadgeLedger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict access badge checkpoints by code.</summary>
public interface IStrictAccessBadgeCheckpointStore
{
    StrictAccessBadgeCheckpoint? Find(string code);
    void Save(StrictAccessBadgeCheckpoint item);
    bool Remove(string code);
    List<StrictAccessBadgeCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictAccessBadgeCheckpointStore(Dictionary<string, StrictAccessBadgeCheckpoint> items, List<string> order) : IStrictAccessBadgeCheckpointStore
{
    public Dictionary<string, StrictAccessBadgeCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictAccessBadgeCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictAccessBadgeCheckpoint item)
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

    public List<StrictAccessBadgeCheckpoint> List()
    {
        var outValue = new List<StrictAccessBadgeCheckpoint>();
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
public sealed class CachedStrictAccessBadgeCheckpointStore(MemoryStrictAccessBadgeCheckpointStore inner, StrictAccessBadgeCheckpointCache cache, int hits, int misses) : IStrictAccessBadgeCheckpointStore
{
    public MemoryStrictAccessBadgeCheckpointStore Inner { get; set; } = inner;
    public StrictAccessBadgeCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictAccessBadgeCheckpoint? Find(string code)
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

    public void Save(StrictAccessBadgeCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictAccessBadgeCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict access badge checkpoints. Empty fields match all.</summary>
public sealed record StrictAccessBadgeCheckpointQuery(string Segment, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(StrictAccessBadgeCheckpoint item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictAccessBadgeCheckpoint> Apply(List<StrictAccessBadgeCheckpoint> items)
    {
        var matched = Collections.Filter(items, (StrictAccessBadgeCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict access badge checkpoint store.</summary>
public static class StrictAccessBadgeCheckpointStore
{
    public static IStrictAccessBadgeCheckpointStore NewMemoryStrictAccessBadgeCheckpointStore()
    {
        return new MemoryStrictAccessBadgeCheckpointStore(
            new Dictionary<string, StrictAccessBadgeCheckpoint>(),
            new List<string>());
    }

    public static IStrictAccessBadgeCheckpointStore NewCachedStrictAccessBadgeCheckpointStore(MemoryStrictAccessBadgeCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedStrictAccessBadgeCheckpointStore(
            inner,
            StrictAccessBadgeCheckpointCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<StrictAccessBadgeCheckpoint> FindStrictAccessBadgeCheckpoints(IStrictAccessBadgeCheckpointStore store, StrictAccessBadgeCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
