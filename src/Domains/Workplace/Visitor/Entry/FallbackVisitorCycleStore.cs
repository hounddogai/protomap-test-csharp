namespace Acme.Workforce.Workplace.Visitor.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback visitor cycles by code.</summary>
public interface IFallbackVisitorCycleStore
{
    FallbackVisitorCycle? Find(string code);
    void Save(FallbackVisitorCycle item);
    bool Remove(string code);
    List<FallbackVisitorCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackVisitorCycleStore(Dictionary<string, FallbackVisitorCycle> items, List<string> order) : IFallbackVisitorCycleStore
{
    public Dictionary<string, FallbackVisitorCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackVisitorCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackVisitorCycle item)
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

    public List<FallbackVisitorCycle> List()
    {
        var outValue = new List<FallbackVisitorCycle>();
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
public sealed class CachedFallbackVisitorCycleStore(MemoryFallbackVisitorCycleStore inner, FallbackVisitorCycleCache cache, int hits, int misses) : IFallbackVisitorCycleStore
{
    public MemoryFallbackVisitorCycleStore Inner { get; set; } = inner;
    public FallbackVisitorCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackVisitorCycle? Find(string code)
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

    public void Save(FallbackVisitorCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackVisitorCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback visitor cycles. Empty fields match all.</summary>
public sealed record FallbackVisitorCycleQuery(string Locale, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackVisitorCycle item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
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

    public List<FallbackVisitorCycle> Apply(List<FallbackVisitorCycle> items)
    {
        var matched = Collections.Filter(items, (FallbackVisitorCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback visitor cycle store.</summary>
public static class FallbackVisitorCycleStore
{
    public static IFallbackVisitorCycleStore NewMemoryFallbackVisitorCycleStore()
    {
        return new MemoryFallbackVisitorCycleStore(
            new Dictionary<string, FallbackVisitorCycle>(),
            new List<string>());
    }

    public static IFallbackVisitorCycleStore NewCachedFallbackVisitorCycleStore(MemoryFallbackVisitorCycleStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackVisitorCycleStore(inner, FallbackVisitorCycleCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<FallbackVisitorCycle> FindFallbackVisitorCycles(IFallbackVisitorCycleStore store, FallbackVisitorCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
