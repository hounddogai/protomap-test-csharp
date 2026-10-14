namespace Acme.Workforce.Compliance.TrainingDue.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy training due batches by code.</summary>
public interface ILegacyTrainingDueBatchStore
{
    LegacyTrainingDueBatch? Find(string code);
    void Save(LegacyTrainingDueBatch item);
    bool Remove(string code);
    List<LegacyTrainingDueBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyTrainingDueBatchStore(Dictionary<string, LegacyTrainingDueBatch> items, List<string> order) : ILegacyTrainingDueBatchStore
{
    public Dictionary<string, LegacyTrainingDueBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyTrainingDueBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyTrainingDueBatch item)
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

    public List<LegacyTrainingDueBatch> List()
    {
        var outValue = new List<LegacyTrainingDueBatch>();
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
public sealed class CachedLegacyTrainingDueBatchStore(MemoryLegacyTrainingDueBatchStore inner, LegacyTrainingDueBatchCache cache, int hits, int misses) : ILegacyTrainingDueBatchStore
{
    public MemoryLegacyTrainingDueBatchStore Inner { get; set; } = inner;
    public LegacyTrainingDueBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyTrainingDueBatch? Find(string code)
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

    public void Save(LegacyTrainingDueBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyTrainingDueBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy training due batches. Empty fields match all.</summary>
public sealed record LegacyTrainingDueBatchQuery(string Region, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyTrainingDueBatch item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyTrainingDueBatch> Apply(List<LegacyTrainingDueBatch> items)
    {
        var matched = Collections.Filter(items, (LegacyTrainingDueBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy training due batch store.</summary>
public static class LegacyTrainingDueBatchStore
{
    public static ILegacyTrainingDueBatchStore NewMemoryLegacyTrainingDueBatchStore()
    {
        return new MemoryLegacyTrainingDueBatchStore(
            new Dictionary<string, LegacyTrainingDueBatch>(),
            new List<string>());
    }

    public static ILegacyTrainingDueBatchStore NewCachedLegacyTrainingDueBatchStore(MemoryLegacyTrainingDueBatchStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyTrainingDueBatchStore(
            inner,
            LegacyTrainingDueBatchCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<LegacyTrainingDueBatch> FindLegacyTrainingDueBatches(ILegacyTrainingDueBatchStore store, LegacyTrainingDueBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
