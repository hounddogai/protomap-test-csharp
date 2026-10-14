namespace Acme.Workforce.Assets.Laptop.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending laptop targets by code.</summary>
public interface IPendingLaptopTargetStore
{
    PendingLaptopTarget? Find(string code);
    void Save(PendingLaptopTarget item);
    bool Remove(string code);
    List<PendingLaptopTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingLaptopTargetStore(Dictionary<string, PendingLaptopTarget> items, List<string> order) : IPendingLaptopTargetStore
{
    public Dictionary<string, PendingLaptopTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingLaptopTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingLaptopTarget item)
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

    public List<PendingLaptopTarget> List()
    {
        var outValue = new List<PendingLaptopTarget>();
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
public sealed class CachedPendingLaptopTargetStore(MemoryPendingLaptopTargetStore inner, PendingLaptopTargetCache cache, int hits, int misses) : IPendingLaptopTargetStore
{
    public MemoryPendingLaptopTargetStore Inner { get; set; } = inner;
    public PendingLaptopTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingLaptopTarget? Find(string code)
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

    public void Save(PendingLaptopTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingLaptopTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending laptop targets. Empty fields match all.</summary>
public sealed record PendingLaptopTargetQuery(string Label, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(PendingLaptopTarget item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingLaptopTarget> Apply(List<PendingLaptopTarget> items)
    {
        var matched = Collections.Filter(items, (PendingLaptopTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending laptop target store.</summary>
public static class PendingLaptopTargetStore
{
    public static IPendingLaptopTargetStore NewMemoryPendingLaptopTargetStore()
    {
        return new MemoryPendingLaptopTargetStore(new Dictionary<string, PendingLaptopTarget>(), new List<string>());
    }

    public static IPendingLaptopTargetStore NewCachedPendingLaptopTargetStore(MemoryPendingLaptopTargetStore inner, int ttl, IClock clock)
    {
        return new CachedPendingLaptopTargetStore(inner, PendingLaptopTargetCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<PendingLaptopTarget> FindPendingLaptopTargets(IPendingLaptopTargetStore store, PendingLaptopTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
