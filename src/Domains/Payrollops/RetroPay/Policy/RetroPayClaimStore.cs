namespace Acme.Workforce.Payrollops.RetroPay.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps retro pay claims by code.</summary>
public interface IRetroPayClaimStore
{
    RetroPayClaim? Find(string code);
    void Save(RetroPayClaim item);
    bool Remove(string code);
    List<RetroPayClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRetroPayClaimStore(Dictionary<string, RetroPayClaim> items, List<string> order) : IRetroPayClaimStore
{
    public Dictionary<string, RetroPayClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RetroPayClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RetroPayClaim item)
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

    public List<RetroPayClaim> List()
    {
        var outValue = new List<RetroPayClaim>();
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
public sealed class CachedRetroPayClaimStore(MemoryRetroPayClaimStore inner, RetroPayClaimCache cache, int hits, int misses) : IRetroPayClaimStore
{
    public MemoryRetroPayClaimStore Inner { get; set; } = inner;
    public RetroPayClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RetroPayClaim? Find(string code)
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

    public void Save(RetroPayClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RetroPayClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects retro pay claims. Empty fields match all.</summary>
public sealed record RetroPayClaimQuery(string Source, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(RetroPayClaim item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
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

    public List<RetroPayClaim> Apply(List<RetroPayClaim> items)
    {
        var matched = Collections.Filter(items, (RetroPayClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of retro pay claim store.</summary>
public static class RetroPayClaimStore
{
    public static IRetroPayClaimStore NewMemoryRetroPayClaimStore()
    {
        return new MemoryRetroPayClaimStore(new Dictionary<string, RetroPayClaim>(), new List<string>());
    }

    public static IRetroPayClaimStore NewCachedRetroPayClaimStore(MemoryRetroPayClaimStore inner, int ttl, IClock clock)
    {
        return new CachedRetroPayClaimStore(inner, RetroPayClaimCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<RetroPayClaim> FindRetroPayClaims(IRetroPayClaimStore store, RetroPayClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
