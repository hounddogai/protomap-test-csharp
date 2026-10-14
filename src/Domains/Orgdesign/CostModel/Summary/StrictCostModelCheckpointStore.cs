namespace Acme.Workforce.Orgdesign.CostModel.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict cost model checkpoints by code.</summary>
public interface IStrictCostModelCheckpointStore
{
    StrictCostModelCheckpoint? Find(string code);
    void Save(StrictCostModelCheckpoint item);
    bool Remove(string code);
    List<StrictCostModelCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictCostModelCheckpointStore(Dictionary<string, StrictCostModelCheckpoint> items, List<string> order) : IStrictCostModelCheckpointStore
{
    public Dictionary<string, StrictCostModelCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictCostModelCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictCostModelCheckpoint item)
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

    public List<StrictCostModelCheckpoint> List()
    {
        var outValue = new List<StrictCostModelCheckpoint>();
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
public sealed class CachedStrictCostModelCheckpointStore(MemoryStrictCostModelCheckpointStore inner, StrictCostModelCheckpointCache cache, int hits, int misses) : IStrictCostModelCheckpointStore
{
    public MemoryStrictCostModelCheckpointStore Inner { get; set; } = inner;
    public StrictCostModelCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictCostModelCheckpoint? Find(string code)
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

    public void Save(StrictCostModelCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictCostModelCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict cost model checkpoints. Empty fields match all.</summary>
public sealed record StrictCostModelCheckpointQuery(string Unit, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(StrictCostModelCheckpoint item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictCostModelCheckpoint> Apply(List<StrictCostModelCheckpoint> items)
    {
        var matched = Collections.Filter(items, (StrictCostModelCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict cost model checkpoint store.</summary>
public static class StrictCostModelCheckpointStore
{
    public static IStrictCostModelCheckpointStore NewMemoryStrictCostModelCheckpointStore()
    {
        return new MemoryStrictCostModelCheckpointStore(
            new Dictionary<string, StrictCostModelCheckpoint>(),
            new List<string>());
    }

    public static IStrictCostModelCheckpointStore NewCachedStrictCostModelCheckpointStore(MemoryStrictCostModelCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedStrictCostModelCheckpointStore(
            inner,
            StrictCostModelCheckpointCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<StrictCostModelCheckpoint> FindStrictCostModelCheckpoints(IStrictCostModelCheckpointStore store, StrictCostModelCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
