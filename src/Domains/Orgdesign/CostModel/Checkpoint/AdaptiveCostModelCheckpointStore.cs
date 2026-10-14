namespace Acme.Workforce.Orgdesign.CostModel.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive cost model checkpoints by code.</summary>
public interface IAdaptiveCostModelCheckpointStore
{
    AdaptiveCostModelCheckpoint? Find(string code);
    void Save(AdaptiveCostModelCheckpoint item);
    bool Remove(string code);
    List<AdaptiveCostModelCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveCostModelCheckpointStore(Dictionary<string, AdaptiveCostModelCheckpoint> items, List<string> order) : IAdaptiveCostModelCheckpointStore
{
    public Dictionary<string, AdaptiveCostModelCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveCostModelCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveCostModelCheckpoint item)
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

    public List<AdaptiveCostModelCheckpoint> List()
    {
        var outValue = new List<AdaptiveCostModelCheckpoint>();
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
public sealed class CachedAdaptiveCostModelCheckpointStore(MemoryAdaptiveCostModelCheckpointStore inner, AdaptiveCostModelCheckpointCache cache, int hits, int misses) : IAdaptiveCostModelCheckpointStore
{
    public MemoryAdaptiveCostModelCheckpointStore Inner { get; set; } = inner;
    public AdaptiveCostModelCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveCostModelCheckpoint? Find(string code)
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

    public void Save(AdaptiveCostModelCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveCostModelCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive cost model checkpoints. Empty fields match all.</summary>
public sealed record AdaptiveCostModelCheckpointQuery(string Unit, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveCostModelCheckpoint item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveCostModelCheckpoint> Apply(List<AdaptiveCostModelCheckpoint> items)
    {
        var matched = Collections.Filter(items, (AdaptiveCostModelCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive cost model checkpoint store.</summary>
public static class AdaptiveCostModelCheckpointStore
{
    public static IAdaptiveCostModelCheckpointStore NewMemoryAdaptiveCostModelCheckpointStore()
    {
        return new MemoryAdaptiveCostModelCheckpointStore(
            new Dictionary<string, AdaptiveCostModelCheckpoint>(),
            new List<string>());
    }

    public static IAdaptiveCostModelCheckpointStore NewCachedAdaptiveCostModelCheckpointStore(MemoryAdaptiveCostModelCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveCostModelCheckpointStore(
            inner,
            AdaptiveCostModelCheckpointCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveCostModelCheckpoint> FindAdaptiveCostModelCheckpoints(IAdaptiveCostModelCheckpointStore store, AdaptiveCostModelCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
