namespace Acme.Workforce.Absence.Accrual.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual accrual checkpoints by code.</summary>
public interface IManualAccrualCheckpointStore
{
    ManualAccrualCheckpoint? Find(string code);
    void Save(ManualAccrualCheckpoint item);
    bool Remove(string code);
    List<ManualAccrualCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualAccrualCheckpointStore(Dictionary<string, ManualAccrualCheckpoint> items, List<string> order) : IManualAccrualCheckpointStore
{
    public Dictionary<string, ManualAccrualCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualAccrualCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualAccrualCheckpoint item)
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

    public List<ManualAccrualCheckpoint> List()
    {
        var outValue = new List<ManualAccrualCheckpoint>();
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
public sealed class CachedManualAccrualCheckpointStore(MemoryManualAccrualCheckpointStore inner, ManualAccrualCheckpointCache cache, int hits, int misses) : IManualAccrualCheckpointStore
{
    public MemoryManualAccrualCheckpointStore Inner { get; set; } = inner;
    public ManualAccrualCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualAccrualCheckpoint? Find(string code)
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

    public void Save(ManualAccrualCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualAccrualCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual accrual checkpoints. Empty fields match all.</summary>
public sealed record ManualAccrualCheckpointQuery(string Category, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(ManualAccrualCheckpoint item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualAccrualCheckpoint> Apply(List<ManualAccrualCheckpoint> items)
    {
        var matched = Collections.Filter(items, (ManualAccrualCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual accrual checkpoint store.</summary>
public static class ManualAccrualCheckpointStore
{
    public static IManualAccrualCheckpointStore NewMemoryManualAccrualCheckpointStore()
    {
        return new MemoryManualAccrualCheckpointStore(
            new Dictionary<string, ManualAccrualCheckpoint>(),
            new List<string>());
    }

    public static IManualAccrualCheckpointStore NewCachedManualAccrualCheckpointStore(MemoryManualAccrualCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedManualAccrualCheckpointStore(
            inner,
            ManualAccrualCheckpointCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<ManualAccrualCheckpoint> FindManualAccrualCheckpoints(IManualAccrualCheckpointStore store, ManualAccrualCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
