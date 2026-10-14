namespace Acme.Workforce.Contractors.Offboarding.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary offboarding batches by code.</summary>
public interface IPrimaryOffboardingBatchStore
{
    PrimaryOffboardingBatch? Find(string code);
    void Save(PrimaryOffboardingBatch item);
    bool Remove(string code);
    List<PrimaryOffboardingBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryOffboardingBatchStore(Dictionary<string, PrimaryOffboardingBatch> items, List<string> order) : IPrimaryOffboardingBatchStore
{
    public Dictionary<string, PrimaryOffboardingBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryOffboardingBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryOffboardingBatch item)
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

    public List<PrimaryOffboardingBatch> List()
    {
        var outValue = new List<PrimaryOffboardingBatch>();
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
public sealed class CachedPrimaryOffboardingBatchStore(MemoryPrimaryOffboardingBatchStore inner, PrimaryOffboardingBatchCache cache, int hits, int misses) : IPrimaryOffboardingBatchStore
{
    public MemoryPrimaryOffboardingBatchStore Inner { get; set; } = inner;
    public PrimaryOffboardingBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryOffboardingBatch? Find(string code)
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

    public void Save(PrimaryOffboardingBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryOffboardingBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary offboarding batches. Empty fields match all.</summary>
public sealed record PrimaryOffboardingBatchQuery(string Name, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryOffboardingBatch item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryOffboardingBatch> Apply(List<PrimaryOffboardingBatch> items)
    {
        var matched = Collections.Filter(items, (PrimaryOffboardingBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary offboarding batch store.</summary>
public static class PrimaryOffboardingBatchStore
{
    public static IPrimaryOffboardingBatchStore NewMemoryPrimaryOffboardingBatchStore()
    {
        return new MemoryPrimaryOffboardingBatchStore(
            new Dictionary<string, PrimaryOffboardingBatch>(),
            new List<string>());
    }

    public static IPrimaryOffboardingBatchStore NewCachedPrimaryOffboardingBatchStore(MemoryPrimaryOffboardingBatchStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryOffboardingBatchStore(
            inner,
            PrimaryOffboardingBatchCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryOffboardingBatch> FindPrimaryOffboardingBatches(IPrimaryOffboardingBatchStore store, PrimaryOffboardingBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
