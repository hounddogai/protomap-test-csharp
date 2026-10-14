namespace Acme.Workforce.Absence.Balance.DefaultBalanceGroup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient balance checkpoints by code.</summary>
public interface ILenientBalanceCheckpointStore
{
    LenientBalanceCheckpoint? Find(string code);
    void Save(LenientBalanceCheckpoint item);
    bool Remove(string code);
    List<LenientBalanceCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientBalanceCheckpointStore(Dictionary<string, LenientBalanceCheckpoint> items, List<string> order) : ILenientBalanceCheckpointStore
{
    public Dictionary<string, LenientBalanceCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientBalanceCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientBalanceCheckpoint item)
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

    public List<LenientBalanceCheckpoint> List()
    {
        var outValue = new List<LenientBalanceCheckpoint>();
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
public sealed class CachedLenientBalanceCheckpointStore(MemoryLenientBalanceCheckpointStore inner, LenientBalanceCheckpointCache cache, int hits, int misses) : ILenientBalanceCheckpointStore
{
    public MemoryLenientBalanceCheckpointStore Inner { get; set; } = inner;
    public LenientBalanceCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientBalanceCheckpoint? Find(string code)
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

    public void Save(LenientBalanceCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientBalanceCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient balance checkpoints. Empty fields match all.</summary>
public sealed record LenientBalanceCheckpointQuery(string Country, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(LenientBalanceCheckpoint item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
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

    public List<LenientBalanceCheckpoint> Apply(List<LenientBalanceCheckpoint> items)
    {
        var matched = Collections.Filter(items, (LenientBalanceCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient balance checkpoint store.</summary>
public static class LenientBalanceCheckpointStore
{
    public static ILenientBalanceCheckpointStore NewMemoryLenientBalanceCheckpointStore()
    {
        return new MemoryLenientBalanceCheckpointStore(
            new Dictionary<string, LenientBalanceCheckpoint>(),
            new List<string>());
    }

    public static ILenientBalanceCheckpointStore NewCachedLenientBalanceCheckpointStore(MemoryLenientBalanceCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedLenientBalanceCheckpointStore(
            inner,
            LenientBalanceCheckpointCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<LenientBalanceCheckpoint> FindLenientBalanceCheckpoints(ILenientBalanceCheckpointStore store, LenientBalanceCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
