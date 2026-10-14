namespace Acme.Workforce.Compensation.EquityGrant.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched equity grant preferences by code.</summary>
public interface IBatchedEquityGrantPreferenceStore
{
    BatchedEquityGrantPreference? Find(string code);
    void Save(BatchedEquityGrantPreference item);
    bool Remove(string code);
    List<BatchedEquityGrantPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedEquityGrantPreferenceStore(Dictionary<string, BatchedEquityGrantPreference> items, List<string> order) : IBatchedEquityGrantPreferenceStore
{
    public Dictionary<string, BatchedEquityGrantPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedEquityGrantPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedEquityGrantPreference item)
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

    public List<BatchedEquityGrantPreference> List()
    {
        var outValue = new List<BatchedEquityGrantPreference>();
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
public sealed class CachedBatchedEquityGrantPreferenceStore(MemoryBatchedEquityGrantPreferenceStore inner, BatchedEquityGrantPreferenceCache cache, int hits, int misses) : IBatchedEquityGrantPreferenceStore
{
    public MemoryBatchedEquityGrantPreferenceStore Inner { get; set; } = inner;
    public BatchedEquityGrantPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedEquityGrantPreference? Find(string code)
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

    public void Save(BatchedEquityGrantPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedEquityGrantPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched equity grant preferences. Empty fields match all.</summary>
public sealed record BatchedEquityGrantPreferenceQuery(string Team, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedEquityGrantPreference item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedEquityGrantPreference> Apply(List<BatchedEquityGrantPreference> items)
    {
        var matched = Collections.Filter(items, (BatchedEquityGrantPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched equity grant preference store.</summary>
public static class BatchedEquityGrantPreferenceStore
{
    public static IBatchedEquityGrantPreferenceStore NewMemoryBatchedEquityGrantPreferenceStore()
    {
        return new MemoryBatchedEquityGrantPreferenceStore(
            new Dictionary<string, BatchedEquityGrantPreference>(),
            new List<string>());
    }

    public static IBatchedEquityGrantPreferenceStore NewCachedBatchedEquityGrantPreferenceStore(MemoryBatchedEquityGrantPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedEquityGrantPreferenceStore(
            inner,
            BatchedEquityGrantPreferenceCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<BatchedEquityGrantPreference> FindBatchedEquityGrantPreferences(IBatchedEquityGrantPreferenceStore store, BatchedEquityGrantPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
