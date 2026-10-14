namespace Acme.Workforce.Compensation.BonusPool.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps bonus pool checkpoints by code.</summary>
public interface IBonusPoolCheckpointStore
{
    BonusPoolCheckpoint? Find(string code);
    void Save(BonusPoolCheckpoint item);
    bool Remove(string code);
    List<BonusPoolCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBonusPoolCheckpointStore(Dictionary<string, BonusPoolCheckpoint> items, List<string> order) : IBonusPoolCheckpointStore
{
    public Dictionary<string, BonusPoolCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BonusPoolCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BonusPoolCheckpoint item)
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

    public List<BonusPoolCheckpoint> List()
    {
        var outValue = new List<BonusPoolCheckpoint>();
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
public sealed class CachedBonusPoolCheckpointStore(MemoryBonusPoolCheckpointStore inner, BonusPoolCheckpointCache cache, int hits, int misses) : IBonusPoolCheckpointStore
{
    public MemoryBonusPoolCheckpointStore Inner { get; set; } = inner;
    public BonusPoolCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BonusPoolCheckpoint? Find(string code)
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

    public void Save(BonusPoolCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BonusPoolCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects bonus pool checkpoints. Empty fields match all.</summary>
public sealed record BonusPoolCheckpointQuery(string Segment, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(BonusPoolCheckpoint item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BonusPoolCheckpoint> Apply(List<BonusPoolCheckpoint> items)
    {
        var matched = Collections.Filter(items, (BonusPoolCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of bonus pool checkpoint store.</summary>
public static class BonusPoolCheckpointStore
{
    public static IBonusPoolCheckpointStore NewMemoryBonusPoolCheckpointStore()
    {
        return new MemoryBonusPoolCheckpointStore(new Dictionary<string, BonusPoolCheckpoint>(), new List<string>());
    }

    public static IBonusPoolCheckpointStore NewCachedBonusPoolCheckpointStore(MemoryBonusPoolCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedBonusPoolCheckpointStore(inner, BonusPoolCheckpointCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<BonusPoolCheckpoint> FindBonusPoolCheckpoints(IBonusPoolCheckpointStore store, BonusPoolCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
