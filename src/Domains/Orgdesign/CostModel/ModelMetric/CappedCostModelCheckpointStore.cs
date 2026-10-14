namespace Acme.Workforce.Orgdesign.CostModel.ModelMetric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped cost model checkpoints by code.</summary>
public interface ICappedCostModelCheckpointStore
{
    CappedCostModelCheckpoint? Find(string code);
    void Save(CappedCostModelCheckpoint item);
    bool Remove(string code);
    List<CappedCostModelCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedCostModelCheckpointStore(Dictionary<string, CappedCostModelCheckpoint> items, List<string> order) : ICappedCostModelCheckpointStore
{
    public Dictionary<string, CappedCostModelCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedCostModelCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedCostModelCheckpoint item)
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

    public List<CappedCostModelCheckpoint> List()
    {
        var outValue = new List<CappedCostModelCheckpoint>();
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
public sealed class CachedCappedCostModelCheckpointStore(MemoryCappedCostModelCheckpointStore inner, CappedCostModelCheckpointCache cache, int hits, int misses) : ICappedCostModelCheckpointStore
{
    public MemoryCappedCostModelCheckpointStore Inner { get; set; } = inner;
    public CappedCostModelCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedCostModelCheckpoint? Find(string code)
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

    public void Save(CappedCostModelCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedCostModelCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped cost model checkpoints. Empty fields match all.</summary>
public sealed record CappedCostModelCheckpointQuery(string Label, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(CappedCostModelCheckpoint item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedCostModelCheckpoint> Apply(List<CappedCostModelCheckpoint> items)
    {
        var matched = Collections.Filter(items, (CappedCostModelCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped cost model checkpoint store.</summary>
public static class CappedCostModelCheckpointStore
{
    public static ICappedCostModelCheckpointStore NewMemoryCappedCostModelCheckpointStore()
    {
        return new MemoryCappedCostModelCheckpointStore(
            new Dictionary<string, CappedCostModelCheckpoint>(),
            new List<string>());
    }

    public static ICappedCostModelCheckpointStore NewCachedCappedCostModelCheckpointStore(MemoryCappedCostModelCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedCappedCostModelCheckpointStore(
            inner,
            CappedCostModelCheckpointCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<CappedCostModelCheckpoint> FindCappedCostModelCheckpoints(ICappedCostModelCheckpointStore store, CappedCostModelCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
