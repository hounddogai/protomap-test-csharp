namespace Acme.Workforce.Compensation.BonusPool.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted bonus pool checkpoints by code.</summary>
public interface IWeightedBonusPoolCheckpointStore
{
    WeightedBonusPoolCheckpoint? Find(string code);
    void Save(WeightedBonusPoolCheckpoint item);
    bool Remove(string code);
    List<WeightedBonusPoolCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedBonusPoolCheckpointStore(Dictionary<string, WeightedBonusPoolCheckpoint> items, List<string> order) : IWeightedBonusPoolCheckpointStore
{
    public Dictionary<string, WeightedBonusPoolCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedBonusPoolCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedBonusPoolCheckpoint item)
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

    public List<WeightedBonusPoolCheckpoint> List()
    {
        var outValue = new List<WeightedBonusPoolCheckpoint>();
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
public sealed class CachedWeightedBonusPoolCheckpointStore(MemoryWeightedBonusPoolCheckpointStore inner, WeightedBonusPoolCheckpointCache cache, int hits, int misses) : IWeightedBonusPoolCheckpointStore
{
    public MemoryWeightedBonusPoolCheckpointStore Inner { get; set; } = inner;
    public WeightedBonusPoolCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedBonusPoolCheckpoint? Find(string code)
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

    public void Save(WeightedBonusPoolCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedBonusPoolCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted bonus pool checkpoints. Empty fields match all.</summary>
public sealed record WeightedBonusPoolCheckpointQuery(string Category, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedBonusPoolCheckpoint item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedBonusPoolCheckpoint> Apply(List<WeightedBonusPoolCheckpoint> items)
    {
        var matched = Collections.Filter(items, (WeightedBonusPoolCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted bonus pool checkpoint store.</summary>
public static class WeightedBonusPoolCheckpointStore
{
    public static IWeightedBonusPoolCheckpointStore NewMemoryWeightedBonusPoolCheckpointStore()
    {
        return new MemoryWeightedBonusPoolCheckpointStore(
            new Dictionary<string, WeightedBonusPoolCheckpoint>(),
            new List<string>());
    }

    public static IWeightedBonusPoolCheckpointStore NewCachedWeightedBonusPoolCheckpointStore(MemoryWeightedBonusPoolCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedBonusPoolCheckpointStore(
            inner,
            WeightedBonusPoolCheckpointCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<WeightedBonusPoolCheckpoint> FindWeightedBonusPoolCheckpoints(IWeightedBonusPoolCheckpointStore store, WeightedBonusPoolCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
