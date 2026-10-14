namespace Acme.Workforce.Compensation.PayBand.Milestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary pay band checkpoints by code.</summary>
public interface IPrimaryPayBandCheckpointStore
{
    PrimaryPayBandCheckpoint? Find(string code);
    void Save(PrimaryPayBandCheckpoint item);
    bool Remove(string code);
    List<PrimaryPayBandCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryPayBandCheckpointStore(Dictionary<string, PrimaryPayBandCheckpoint> items, List<string> order) : IPrimaryPayBandCheckpointStore
{
    public Dictionary<string, PrimaryPayBandCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryPayBandCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryPayBandCheckpoint item)
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

    public List<PrimaryPayBandCheckpoint> List()
    {
        var outValue = new List<PrimaryPayBandCheckpoint>();
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
public sealed class CachedPrimaryPayBandCheckpointStore(MemoryPrimaryPayBandCheckpointStore inner, PrimaryPayBandCheckpointCache cache, int hits, int misses) : IPrimaryPayBandCheckpointStore
{
    public MemoryPrimaryPayBandCheckpointStore Inner { get; set; } = inner;
    public PrimaryPayBandCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryPayBandCheckpoint? Find(string code)
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

    public void Save(PrimaryPayBandCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryPayBandCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary pay band checkpoints. Empty fields match all.</summary>
public sealed record PrimaryPayBandCheckpointQuery(string Label, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryPayBandCheckpoint item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryPayBandCheckpoint> Apply(List<PrimaryPayBandCheckpoint> items)
    {
        var matched = Collections.Filter(items, (PrimaryPayBandCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary pay band checkpoint store.</summary>
public static class PrimaryPayBandCheckpointStore
{
    public static IPrimaryPayBandCheckpointStore NewMemoryPrimaryPayBandCheckpointStore()
    {
        return new MemoryPrimaryPayBandCheckpointStore(
            new Dictionary<string, PrimaryPayBandCheckpoint>(),
            new List<string>());
    }

    public static IPrimaryPayBandCheckpointStore NewCachedPrimaryPayBandCheckpointStore(MemoryPrimaryPayBandCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryPayBandCheckpointStore(
            inner,
            PrimaryPayBandCheckpointCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryPayBandCheckpoint> FindPrimaryPayBandCheckpoints(IPrimaryPayBandCheckpointStore store, PrimaryPayBandCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
