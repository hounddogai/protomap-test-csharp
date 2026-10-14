namespace Acme.Workforce.Talent.Mobility.MobilityAudit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched mobility profiles by code.</summary>
public interface IBatchedMobilityProfileStore
{
    BatchedMobilityProfile? Find(string code);
    void Save(BatchedMobilityProfile item);
    bool Remove(string code);
    List<BatchedMobilityProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedMobilityProfileStore(Dictionary<string, BatchedMobilityProfile> items, List<string> order) : IBatchedMobilityProfileStore
{
    public Dictionary<string, BatchedMobilityProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedMobilityProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedMobilityProfile item)
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

    public List<BatchedMobilityProfile> List()
    {
        var outValue = new List<BatchedMobilityProfile>();
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
public sealed class CachedBatchedMobilityProfileStore(MemoryBatchedMobilityProfileStore inner, BatchedMobilityProfileCache cache, int hits, int misses) : IBatchedMobilityProfileStore
{
    public MemoryBatchedMobilityProfileStore Inner { get; set; } = inner;
    public BatchedMobilityProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedMobilityProfile? Find(string code)
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

    public void Save(BatchedMobilityProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedMobilityProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched mobility profiles. Empty fields match all.</summary>
public sealed record BatchedMobilityProfileQuery(string Source, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedMobilityProfile item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedMobilityProfile> Apply(List<BatchedMobilityProfile> items)
    {
        var matched = Collections.Filter(items, (BatchedMobilityProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched mobility profile store.</summary>
public static class BatchedMobilityProfileStore
{
    public static IBatchedMobilityProfileStore NewMemoryBatchedMobilityProfileStore()
    {
        return new MemoryBatchedMobilityProfileStore(
            new Dictionary<string, BatchedMobilityProfile>(),
            new List<string>());
    }

    public static IBatchedMobilityProfileStore NewCachedBatchedMobilityProfileStore(MemoryBatchedMobilityProfileStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedMobilityProfileStore(
            inner,
            BatchedMobilityProfileCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<BatchedMobilityProfile> FindBatchedMobilityProfiles(IBatchedMobilityProfileStore store, BatchedMobilityProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
