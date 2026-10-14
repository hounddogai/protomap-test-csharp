namespace Acme.Workforce.Orgdesign.Span.Claim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending span claims by code.</summary>
public interface IPendingSpanClaimStore
{
    PendingSpanClaim? Find(string code);
    void Save(PendingSpanClaim item);
    bool Remove(string code);
    List<PendingSpanClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingSpanClaimStore(Dictionary<string, PendingSpanClaim> items, List<string> order) : IPendingSpanClaimStore
{
    public Dictionary<string, PendingSpanClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingSpanClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingSpanClaim item)
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

    public List<PendingSpanClaim> List()
    {
        var outValue = new List<PendingSpanClaim>();
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
public sealed class CachedPendingSpanClaimStore(MemoryPendingSpanClaimStore inner, PendingSpanClaimCache cache, int hits, int misses) : IPendingSpanClaimStore
{
    public MemoryPendingSpanClaimStore Inner { get; set; } = inner;
    public PendingSpanClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingSpanClaim? Find(string code)
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

    public void Save(PendingSpanClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingSpanClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending span claims. Empty fields match all.</summary>
public sealed record PendingSpanClaimQuery(string Country, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(PendingSpanClaim item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingSpanClaim> Apply(List<PendingSpanClaim> items)
    {
        var matched = Collections.Filter(items, (PendingSpanClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending span claim store.</summary>
public static class PendingSpanClaimStore
{
    public static IPendingSpanClaimStore NewMemoryPendingSpanClaimStore()
    {
        return new MemoryPendingSpanClaimStore(new Dictionary<string, PendingSpanClaim>(), new List<string>());
    }

    public static IPendingSpanClaimStore NewCachedPendingSpanClaimStore(MemoryPendingSpanClaimStore inner, int ttl, IClock clock)
    {
        return new CachedPendingSpanClaimStore(inner, PendingSpanClaimCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PendingSpanClaim> FindPendingSpanClaims(IPendingSpanClaimStore store, PendingSpanClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
