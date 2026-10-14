namespace Acme.Workforce.Assets.Refresh.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending refresh claims by code.</summary>
public interface IPendingRefreshClaimStore
{
    PendingRefreshClaim? Find(string code);
    void Save(PendingRefreshClaim item);
    bool Remove(string code);
    List<PendingRefreshClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingRefreshClaimStore(Dictionary<string, PendingRefreshClaim> items, List<string> order) : IPendingRefreshClaimStore
{
    public Dictionary<string, PendingRefreshClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingRefreshClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingRefreshClaim item)
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

    public List<PendingRefreshClaim> List()
    {
        var outValue = new List<PendingRefreshClaim>();
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
public sealed class CachedPendingRefreshClaimStore(MemoryPendingRefreshClaimStore inner, PendingRefreshClaimCache cache, int hits, int misses) : IPendingRefreshClaimStore
{
    public MemoryPendingRefreshClaimStore Inner { get; set; } = inner;
    public PendingRefreshClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingRefreshClaim? Find(string code)
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

    public void Save(PendingRefreshClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingRefreshClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending refresh claims. Empty fields match all.</summary>
public sealed record PendingRefreshClaimQuery(string Country, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(PendingRefreshClaim item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingRefreshClaim> Apply(List<PendingRefreshClaim> items)
    {
        var matched = Collections.Filter(items, (PendingRefreshClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending refresh claim store.</summary>
public static class PendingRefreshClaimStore
{
    public static IPendingRefreshClaimStore NewMemoryPendingRefreshClaimStore()
    {
        return new MemoryPendingRefreshClaimStore(new Dictionary<string, PendingRefreshClaim>(), new List<string>());
    }

    public static IPendingRefreshClaimStore NewCachedPendingRefreshClaimStore(MemoryPendingRefreshClaimStore inner, int ttl, IClock clock)
    {
        return new CachedPendingRefreshClaimStore(inner, PendingRefreshClaimCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<PendingRefreshClaim> FindPendingRefreshClaims(IPendingRefreshClaimStore store, PendingRefreshClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
