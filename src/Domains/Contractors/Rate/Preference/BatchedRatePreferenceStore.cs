namespace Acme.Workforce.Contractors.Rate.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched rate preferences by code.</summary>
public interface IBatchedRatePreferenceStore
{
    BatchedRatePreference? Find(string code);
    void Save(BatchedRatePreference item);
    bool Remove(string code);
    List<BatchedRatePreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedRatePreferenceStore(Dictionary<string, BatchedRatePreference> items, List<string> order) : IBatchedRatePreferenceStore
{
    public Dictionary<string, BatchedRatePreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedRatePreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedRatePreference item)
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

    public List<BatchedRatePreference> List()
    {
        var outValue = new List<BatchedRatePreference>();
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
public sealed class CachedBatchedRatePreferenceStore(MemoryBatchedRatePreferenceStore inner, BatchedRatePreferenceCache cache, int hits, int misses) : IBatchedRatePreferenceStore
{
    public MemoryBatchedRatePreferenceStore Inner { get; set; } = inner;
    public BatchedRatePreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedRatePreference? Find(string code)
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

    public void Save(BatchedRatePreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedRatePreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched rate preferences. Empty fields match all.</summary>
public sealed record BatchedRatePreferenceQuery(string Segment, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedRatePreference item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedRatePreference> Apply(List<BatchedRatePreference> items)
    {
        var matched = Collections.Filter(items, (BatchedRatePreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched rate preference store.</summary>
public static class BatchedRatePreferenceStore
{
    public static IBatchedRatePreferenceStore NewMemoryBatchedRatePreferenceStore()
    {
        return new MemoryBatchedRatePreferenceStore(
            new Dictionary<string, BatchedRatePreference>(),
            new List<string>());
    }

    public static IBatchedRatePreferenceStore NewCachedBatchedRatePreferenceStore(MemoryBatchedRatePreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedRatePreferenceStore(inner, BatchedRatePreferenceCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<BatchedRatePreference> FindBatchedRatePreferences(IBatchedRatePreferenceStore store, BatchedRatePreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
