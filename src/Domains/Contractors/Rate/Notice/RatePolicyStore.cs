namespace Acme.Workforce.Contractors.Rate.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rate policies by code.</summary>
public interface IRatePolicyStore
{
    RatePolicy? Find(string code);
    void Save(RatePolicy item);
    bool Remove(string code);
    List<RatePolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRatePolicyStore(Dictionary<string, RatePolicy> items, List<string> order) : IRatePolicyStore
{
    public Dictionary<string, RatePolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RatePolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RatePolicy item)
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

    public List<RatePolicy> List()
    {
        var outValue = new List<RatePolicy>();
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
public sealed class CachedRatePolicyStore(MemoryRatePolicyStore inner, RatePolicyCache cache, int hits, int misses) : IRatePolicyStore
{
    public MemoryRatePolicyStore Inner { get; set; } = inner;
    public RatePolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RatePolicy? Find(string code)
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

    public void Save(RatePolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RatePolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rate policies. Empty fields match all.</summary>
public sealed record RatePolicyQuery(string Team, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(RatePolicy item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RatePolicy> Apply(List<RatePolicy> items)
    {
        var matched = Collections.Filter(items, (RatePolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rate policy store.</summary>
public static class RatePolicyStore
{
    public static IRatePolicyStore NewMemoryRatePolicyStore()
    {
        return new MemoryRatePolicyStore(new Dictionary<string, RatePolicy>(), new List<string>());
    }

    public static IRatePolicyStore NewCachedRatePolicyStore(MemoryRatePolicyStore inner, int ttl, IClock clock)
    {
        return new CachedRatePolicyStore(inner, RatePolicyCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<RatePolicy> FindRatePolicies(IRatePolicyStore store, RatePolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
