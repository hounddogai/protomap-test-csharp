namespace Acme.Workforce.Absence.Balance.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps balance checkpoints by code.</summary>
public interface IBalanceCheckpointStore
{
    BalanceCheckpoint? Find(string code);
    void Save(BalanceCheckpoint item);
    bool Remove(string code);
    List<BalanceCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBalanceCheckpointStore(Dictionary<string, BalanceCheckpoint> items, List<string> order) : IBalanceCheckpointStore
{
    public Dictionary<string, BalanceCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BalanceCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BalanceCheckpoint item)
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

    public List<BalanceCheckpoint> List()
    {
        var outValue = new List<BalanceCheckpoint>();
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
public sealed class CachedBalanceCheckpointStore(MemoryBalanceCheckpointStore inner, BalanceCheckpointCache cache, int hits, int misses) : IBalanceCheckpointStore
{
    public MemoryBalanceCheckpointStore Inner { get; set; } = inner;
    public BalanceCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BalanceCheckpoint? Find(string code)
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

    public void Save(BalanceCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BalanceCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects balance checkpoints. Empty fields match all.</summary>
public sealed record BalanceCheckpointQuery(string Name, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(BalanceCheckpoint item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BalanceCheckpoint> Apply(List<BalanceCheckpoint> items)
    {
        var matched = Collections.Filter(items, (BalanceCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of balance checkpoint store.</summary>
public static class BalanceCheckpointStore
{
    public static IBalanceCheckpointStore NewMemoryBalanceCheckpointStore()
    {
        return new MemoryBalanceCheckpointStore(new Dictionary<string, BalanceCheckpoint>(), new List<string>());
    }

    public static IBalanceCheckpointStore NewCachedBalanceCheckpointStore(MemoryBalanceCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedBalanceCheckpointStore(inner, BalanceCheckpointCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<BalanceCheckpoint> FindBalanceCheckpoints(IBalanceCheckpointStore store, BalanceCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
