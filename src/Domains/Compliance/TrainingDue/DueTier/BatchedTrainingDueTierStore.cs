namespace Acme.Workforce.Compliance.TrainingDue.DueTier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched training due tiers by code.</summary>
public interface IBatchedTrainingDueTierStore
{
    BatchedTrainingDueTier? Find(string code);
    void Save(BatchedTrainingDueTier item);
    bool Remove(string code);
    List<BatchedTrainingDueTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedTrainingDueTierStore(Dictionary<string, BatchedTrainingDueTier> items, List<string> order) : IBatchedTrainingDueTierStore
{
    public Dictionary<string, BatchedTrainingDueTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedTrainingDueTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedTrainingDueTier item)
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

    public List<BatchedTrainingDueTier> List()
    {
        var outValue = new List<BatchedTrainingDueTier>();
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
public sealed class CachedBatchedTrainingDueTierStore(MemoryBatchedTrainingDueTierStore inner, BatchedTrainingDueTierCache cache, int hits, int misses) : IBatchedTrainingDueTierStore
{
    public MemoryBatchedTrainingDueTierStore Inner { get; set; } = inner;
    public BatchedTrainingDueTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedTrainingDueTier? Find(string code)
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

    public void Save(BatchedTrainingDueTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedTrainingDueTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched training due tiers. Empty fields match all.</summary>
public sealed record BatchedTrainingDueTierQuery(string Label, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedTrainingDueTier item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedTrainingDueTier> Apply(List<BatchedTrainingDueTier> items)
    {
        var matched = Collections.Filter(items, (BatchedTrainingDueTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched training due tier store.</summary>
public static class BatchedTrainingDueTierStore
{
    public static IBatchedTrainingDueTierStore NewMemoryBatchedTrainingDueTierStore()
    {
        return new MemoryBatchedTrainingDueTierStore(
            new Dictionary<string, BatchedTrainingDueTier>(),
            new List<string>());
    }

    public static IBatchedTrainingDueTierStore NewCachedBatchedTrainingDueTierStore(MemoryBatchedTrainingDueTierStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedTrainingDueTierStore(
            inner,
            BatchedTrainingDueTierCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<BatchedTrainingDueTier> FindBatchedTrainingDueTiers(IBatchedTrainingDueTierStore store, BatchedTrainingDueTierQuery query)
    {
        return query.Apply(store.List());
    }
}
