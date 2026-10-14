namespace Acme.Workforce.Payrollops.PayRun.RunAllocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback pay run cycles by code.</summary>
public interface IFallbackPayRunCycleStore
{
    FallbackPayRunCycle? Find(string code);
    void Save(FallbackPayRunCycle item);
    bool Remove(string code);
    List<FallbackPayRunCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackPayRunCycleStore(Dictionary<string, FallbackPayRunCycle> items, List<string> order) : IFallbackPayRunCycleStore
{
    public Dictionary<string, FallbackPayRunCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackPayRunCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackPayRunCycle item)
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

    public List<FallbackPayRunCycle> List()
    {
        var outValue = new List<FallbackPayRunCycle>();
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
public sealed class CachedFallbackPayRunCycleStore(MemoryFallbackPayRunCycleStore inner, FallbackPayRunCycleCache cache, int hits, int misses) : IFallbackPayRunCycleStore
{
    public MemoryFallbackPayRunCycleStore Inner { get; set; } = inner;
    public FallbackPayRunCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackPayRunCycle? Find(string code)
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

    public void Save(FallbackPayRunCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackPayRunCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback pay run cycles. Empty fields match all.</summary>
public sealed record FallbackPayRunCycleQuery(string Source, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackPayRunCycle item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
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

    public List<FallbackPayRunCycle> Apply(List<FallbackPayRunCycle> items)
    {
        var matched = Collections.Filter(items, (FallbackPayRunCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback pay run cycle store.</summary>
public static class FallbackPayRunCycleStore
{
    public static IFallbackPayRunCycleStore NewMemoryFallbackPayRunCycleStore()
    {
        return new MemoryFallbackPayRunCycleStore(new Dictionary<string, FallbackPayRunCycle>(), new List<string>());
    }

    public static IFallbackPayRunCycleStore NewCachedFallbackPayRunCycleStore(MemoryFallbackPayRunCycleStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackPayRunCycleStore(inner, FallbackPayRunCycleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<FallbackPayRunCycle> FindFallbackPayRunCycles(IFallbackPayRunCycleStore store, FallbackPayRunCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
