namespace Acme.Workforce.Contractors.Rate.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rate claims by code.</summary>
public interface IRateClaimStore
{
    RateClaim? Find(string code);
    void Save(RateClaim item);
    bool Remove(string code);
    List<RateClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRateClaimStore(Dictionary<string, RateClaim> items, List<string> order) : IRateClaimStore
{
    public Dictionary<string, RateClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RateClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RateClaim item)
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

    public List<RateClaim> List()
    {
        var outValue = new List<RateClaim>();
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
public sealed class CachedRateClaimStore(MemoryRateClaimStore inner, RateClaimCache cache, int hits, int misses) : IRateClaimStore
{
    public MemoryRateClaimStore Inner { get; set; } = inner;
    public RateClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RateClaim? Find(string code)
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

    public void Save(RateClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RateClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rate claims. Empty fields match all.</summary>
public sealed record RateClaimQuery(string Name, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(RateClaim item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<RateClaim> Apply(List<RateClaim> items)
    {
        var matched = Collections.Filter(items, (RateClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rate claim store.</summary>
public static class RateClaimStore
{
    public static IRateClaimStore NewMemoryRateClaimStore()
    {
        return new MemoryRateClaimStore(new Dictionary<string, RateClaim>(), new List<string>());
    }

    public static IRateClaimStore NewCachedRateClaimStore(MemoryRateClaimStore inner, int ttl, IClock clock)
    {
        return new CachedRateClaimStore(inner, RateClaimCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<RateClaim> FindRateClaims(IRateClaimStore store, RateClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
