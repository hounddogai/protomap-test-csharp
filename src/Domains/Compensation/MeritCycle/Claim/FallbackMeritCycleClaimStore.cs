namespace Acme.Workforce.Compensation.MeritCycle.Claim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback merit cycle claims by code.</summary>
public interface IFallbackMeritCycleClaimStore
{
    FallbackMeritCycleClaim? Find(string code);
    void Save(FallbackMeritCycleClaim item);
    bool Remove(string code);
    List<FallbackMeritCycleClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackMeritCycleClaimStore(Dictionary<string, FallbackMeritCycleClaim> items, List<string> order) : IFallbackMeritCycleClaimStore
{
    public Dictionary<string, FallbackMeritCycleClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackMeritCycleClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackMeritCycleClaim item)
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

    public List<FallbackMeritCycleClaim> List()
    {
        var outValue = new List<FallbackMeritCycleClaim>();
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
public sealed class CachedFallbackMeritCycleClaimStore(MemoryFallbackMeritCycleClaimStore inner, FallbackMeritCycleClaimCache cache, int hits, int misses) : IFallbackMeritCycleClaimStore
{
    public MemoryFallbackMeritCycleClaimStore Inner { get; set; } = inner;
    public FallbackMeritCycleClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackMeritCycleClaim? Find(string code)
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

    public void Save(FallbackMeritCycleClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackMeritCycleClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback merit cycle claims. Empty fields match all.</summary>
public sealed record FallbackMeritCycleClaimQuery(string Segment, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackMeritCycleClaim item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackMeritCycleClaim> Apply(List<FallbackMeritCycleClaim> items)
    {
        var matched = Collections.Filter(items, (FallbackMeritCycleClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback merit cycle claim store.</summary>
public static class FallbackMeritCycleClaimStore
{
    public static IFallbackMeritCycleClaimStore NewMemoryFallbackMeritCycleClaimStore()
    {
        return new MemoryFallbackMeritCycleClaimStore(
            new Dictionary<string, FallbackMeritCycleClaim>(),
            new List<string>());
    }

    public static IFallbackMeritCycleClaimStore NewCachedFallbackMeritCycleClaimStore(MemoryFallbackMeritCycleClaimStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackMeritCycleClaimStore(
            inner,
            FallbackMeritCycleClaimCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<FallbackMeritCycleClaim> FindFallbackMeritCycleClaims(IFallbackMeritCycleClaimStore store, FallbackMeritCycleClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
