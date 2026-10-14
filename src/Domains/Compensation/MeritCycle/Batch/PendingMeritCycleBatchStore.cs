namespace Acme.Workforce.Compensation.MeritCycle.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending merit cycle batches by code.</summary>
public interface IPendingMeritCycleBatchStore
{
    PendingMeritCycleBatch? Find(string code);
    void Save(PendingMeritCycleBatch item);
    bool Remove(string code);
    List<PendingMeritCycleBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingMeritCycleBatchStore(Dictionary<string, PendingMeritCycleBatch> items, List<string> order) : IPendingMeritCycleBatchStore
{
    public Dictionary<string, PendingMeritCycleBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingMeritCycleBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingMeritCycleBatch item)
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

    public List<PendingMeritCycleBatch> List()
    {
        var outValue = new List<PendingMeritCycleBatch>();
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
public sealed class CachedPendingMeritCycleBatchStore(MemoryPendingMeritCycleBatchStore inner, PendingMeritCycleBatchCache cache, int hits, int misses) : IPendingMeritCycleBatchStore
{
    public MemoryPendingMeritCycleBatchStore Inner { get; set; } = inner;
    public PendingMeritCycleBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingMeritCycleBatch? Find(string code)
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

    public void Save(PendingMeritCycleBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingMeritCycleBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending merit cycle batches. Empty fields match all.</summary>
public sealed record PendingMeritCycleBatchQuery(string Region, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(PendingMeritCycleBatch item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingMeritCycleBatch> Apply(List<PendingMeritCycleBatch> items)
    {
        var matched = Collections.Filter(items, (PendingMeritCycleBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending merit cycle batch store.</summary>
public static class PendingMeritCycleBatchStore
{
    public static IPendingMeritCycleBatchStore NewMemoryPendingMeritCycleBatchStore()
    {
        return new MemoryPendingMeritCycleBatchStore(
            new Dictionary<string, PendingMeritCycleBatch>(),
            new List<string>());
    }

    public static IPendingMeritCycleBatchStore NewCachedPendingMeritCycleBatchStore(MemoryPendingMeritCycleBatchStore inner, int ttl, IClock clock)
    {
        return new CachedPendingMeritCycleBatchStore(
            inner,
            PendingMeritCycleBatchCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<PendingMeritCycleBatch> FindPendingMeritCycleBatches(IPendingMeritCycleBatchStore store, PendingMeritCycleBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
