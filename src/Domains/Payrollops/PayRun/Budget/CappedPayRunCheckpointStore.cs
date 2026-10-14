namespace Acme.Workforce.Payrollops.PayRun.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped pay run checkpoints by code.</summary>
public interface ICappedPayRunCheckpointStore
{
    CappedPayRunCheckpoint? Find(string code);
    void Save(CappedPayRunCheckpoint item);
    bool Remove(string code);
    List<CappedPayRunCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedPayRunCheckpointStore(Dictionary<string, CappedPayRunCheckpoint> items, List<string> order) : ICappedPayRunCheckpointStore
{
    public Dictionary<string, CappedPayRunCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedPayRunCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedPayRunCheckpoint item)
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

    public List<CappedPayRunCheckpoint> List()
    {
        var outValue = new List<CappedPayRunCheckpoint>();
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
public sealed class CachedCappedPayRunCheckpointStore(MemoryCappedPayRunCheckpointStore inner, CappedPayRunCheckpointCache cache, int hits, int misses) : ICappedPayRunCheckpointStore
{
    public MemoryCappedPayRunCheckpointStore Inner { get; set; } = inner;
    public CappedPayRunCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedPayRunCheckpoint? Find(string code)
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

    public void Save(CappedPayRunCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedPayRunCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped pay run checkpoints. Empty fields match all.</summary>
public sealed record CappedPayRunCheckpointQuery(string Region, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(CappedPayRunCheckpoint item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedPayRunCheckpoint> Apply(List<CappedPayRunCheckpoint> items)
    {
        var matched = Collections.Filter(items, (CappedPayRunCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped pay run checkpoint store.</summary>
public static class CappedPayRunCheckpointStore
{
    public static ICappedPayRunCheckpointStore NewMemoryCappedPayRunCheckpointStore()
    {
        return new MemoryCappedPayRunCheckpointStore(
            new Dictionary<string, CappedPayRunCheckpoint>(),
            new List<string>());
    }

    public static ICappedPayRunCheckpointStore NewCachedCappedPayRunCheckpointStore(MemoryCappedPayRunCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedCappedPayRunCheckpointStore(
            inner,
            CappedPayRunCheckpointCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<CappedPayRunCheckpoint> FindCappedPayRunCheckpoints(ICappedPayRunCheckpointStore store, CappedPayRunCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
