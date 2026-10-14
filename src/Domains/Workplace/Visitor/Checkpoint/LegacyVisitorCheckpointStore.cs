namespace Acme.Workforce.Workplace.Visitor.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy visitor checkpoints by code.</summary>
public interface ILegacyVisitorCheckpointStore
{
    LegacyVisitorCheckpoint? Find(string code);
    void Save(LegacyVisitorCheckpoint item);
    bool Remove(string code);
    List<LegacyVisitorCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyVisitorCheckpointStore(Dictionary<string, LegacyVisitorCheckpoint> items, List<string> order) : ILegacyVisitorCheckpointStore
{
    public Dictionary<string, LegacyVisitorCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyVisitorCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyVisitorCheckpoint item)
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

    public List<LegacyVisitorCheckpoint> List()
    {
        var outValue = new List<LegacyVisitorCheckpoint>();
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
public sealed class CachedLegacyVisitorCheckpointStore(MemoryLegacyVisitorCheckpointStore inner, LegacyVisitorCheckpointCache cache, int hits, int misses) : ILegacyVisitorCheckpointStore
{
    public MemoryLegacyVisitorCheckpointStore Inner { get; set; } = inner;
    public LegacyVisitorCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyVisitorCheckpoint? Find(string code)
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

    public void Save(LegacyVisitorCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyVisitorCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy visitor checkpoints. Empty fields match all.</summary>
public sealed record LegacyVisitorCheckpointQuery(string Department, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyVisitorCheckpoint item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyVisitorCheckpoint> Apply(List<LegacyVisitorCheckpoint> items)
    {
        var matched = Collections.Filter(items, (LegacyVisitorCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy visitor checkpoint store.</summary>
public static class LegacyVisitorCheckpointStore
{
    public static ILegacyVisitorCheckpointStore NewMemoryLegacyVisitorCheckpointStore()
    {
        return new MemoryLegacyVisitorCheckpointStore(
            new Dictionary<string, LegacyVisitorCheckpoint>(),
            new List<string>());
    }

    public static ILegacyVisitorCheckpointStore NewCachedLegacyVisitorCheckpointStore(MemoryLegacyVisitorCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyVisitorCheckpointStore(
            inner,
            LegacyVisitorCheckpointCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<LegacyVisitorCheckpoint> FindLegacyVisitorCheckpoints(ILegacyVisitorCheckpointStore store, LegacyVisitorCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
