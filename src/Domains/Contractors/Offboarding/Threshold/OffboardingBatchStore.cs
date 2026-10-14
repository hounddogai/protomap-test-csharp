namespace Acme.Workforce.Contractors.Offboarding.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps offboarding batches by code.</summary>
public interface IOffboardingBatchStore
{
    OffboardingBatch? Find(string code);
    void Save(OffboardingBatch item);
    bool Remove(string code);
    List<OffboardingBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOffboardingBatchStore(Dictionary<string, OffboardingBatch> items, List<string> order) : IOffboardingBatchStore
{
    public Dictionary<string, OffboardingBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OffboardingBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OffboardingBatch item)
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

    public List<OffboardingBatch> List()
    {
        var outValue = new List<OffboardingBatch>();
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
public sealed class CachedOffboardingBatchStore(MemoryOffboardingBatchStore inner, OffboardingBatchCache cache, int hits, int misses) : IOffboardingBatchStore
{
    public MemoryOffboardingBatchStore Inner { get; set; } = inner;
    public OffboardingBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OffboardingBatch? Find(string code)
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

    public void Save(OffboardingBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OffboardingBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects offboarding batches. Empty fields match all.</summary>
public sealed record OffboardingBatchQuery(string Source, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(OffboardingBatch item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<OffboardingBatch> Apply(List<OffboardingBatch> items)
    {
        var matched = Collections.Filter(items, (OffboardingBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of offboarding batch store.</summary>
public static class OffboardingBatchStore
{
    public static IOffboardingBatchStore NewMemoryOffboardingBatchStore()
    {
        return new MemoryOffboardingBatchStore(new Dictionary<string, OffboardingBatch>(), new List<string>());
    }

    public static IOffboardingBatchStore NewCachedOffboardingBatchStore(MemoryOffboardingBatchStore inner, int ttl, IClock clock)
    {
        return new CachedOffboardingBatchStore(inner, OffboardingBatchCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<OffboardingBatch> FindOffboardingBatches(IOffboardingBatchStore store, OffboardingBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
