namespace Acme.Workforce.Orgdesign.Reorg.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps reorg checkpoints by code.</summary>
public interface IReorgCheckpointStore
{
    ReorgCheckpoint? Find(string code);
    void Save(ReorgCheckpoint item);
    bool Remove(string code);
    List<ReorgCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReorgCheckpointStore(Dictionary<string, ReorgCheckpoint> items, List<string> order) : IReorgCheckpointStore
{
    public Dictionary<string, ReorgCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReorgCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReorgCheckpoint item)
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

    public List<ReorgCheckpoint> List()
    {
        var outValue = new List<ReorgCheckpoint>();
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
public sealed class CachedReorgCheckpointStore(MemoryReorgCheckpointStore inner, ReorgCheckpointCache cache, int hits, int misses) : IReorgCheckpointStore
{
    public MemoryReorgCheckpointStore Inner { get; set; } = inner;
    public ReorgCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReorgCheckpoint? Find(string code)
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

    public void Save(ReorgCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReorgCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects reorg checkpoints. Empty fields match all.</summary>
public sealed record ReorgCheckpointQuery(string Source, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(ReorgCheckpoint item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
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

    public List<ReorgCheckpoint> Apply(List<ReorgCheckpoint> items)
    {
        var matched = Collections.Filter(items, (ReorgCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of reorg checkpoint store.</summary>
public static class ReorgCheckpointStore
{
    public static IReorgCheckpointStore NewMemoryReorgCheckpointStore()
    {
        return new MemoryReorgCheckpointStore(new Dictionary<string, ReorgCheckpoint>(), new List<string>());
    }

    public static IReorgCheckpointStore NewCachedReorgCheckpointStore(MemoryReorgCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedReorgCheckpointStore(inner, ReorgCheckpointCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<ReorgCheckpoint> FindReorgCheckpoints(IReorgCheckpointStore store, ReorgCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
