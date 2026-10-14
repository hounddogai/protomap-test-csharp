namespace Acme.Workforce.Compensation.MeritCycle.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps merit cycle batches by code.</summary>
public interface IMeritCycleBatchStore
{
    MeritCycleBatch? Find(string code);
    void Save(MeritCycleBatch item);
    bool Remove(string code);
    List<MeritCycleBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMeritCycleBatchStore(Dictionary<string, MeritCycleBatch> items, List<string> order) : IMeritCycleBatchStore
{
    public Dictionary<string, MeritCycleBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MeritCycleBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MeritCycleBatch item)
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

    public List<MeritCycleBatch> List()
    {
        var outValue = new List<MeritCycleBatch>();
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
public sealed class CachedMeritCycleBatchStore(MemoryMeritCycleBatchStore inner, MeritCycleBatchCache cache, int hits, int misses) : IMeritCycleBatchStore
{
    public MemoryMeritCycleBatchStore Inner { get; set; } = inner;
    public MeritCycleBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MeritCycleBatch? Find(string code)
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

    public void Save(MeritCycleBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MeritCycleBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects merit cycle batches. Empty fields match all.</summary>
public sealed record MeritCycleBatchQuery(string Region, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(MeritCycleBatch item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<MeritCycleBatch> Apply(List<MeritCycleBatch> items)
    {
        var matched = Collections.Filter(items, (MeritCycleBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of merit cycle batch store.</summary>
public static class MeritCycleBatchStore
{
    public static IMeritCycleBatchStore NewMemoryMeritCycleBatchStore()
    {
        return new MemoryMeritCycleBatchStore(new Dictionary<string, MeritCycleBatch>(), new List<string>());
    }

    public static IMeritCycleBatchStore NewCachedMeritCycleBatchStore(MemoryMeritCycleBatchStore inner, int ttl, IClock clock)
    {
        return new CachedMeritCycleBatchStore(inner, MeritCycleBatchCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<MeritCycleBatch> FindMeritCycleBatches(IMeritCycleBatchStore store, MeritCycleBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
