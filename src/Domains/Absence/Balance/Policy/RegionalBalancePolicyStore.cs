namespace Acme.Workforce.Absence.Balance.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional balance policies by code.</summary>
public interface IRegionalBalancePolicyStore
{
    RegionalBalancePolicy? Find(string code);
    void Save(RegionalBalancePolicy item);
    bool Remove(string code);
    List<RegionalBalancePolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalBalancePolicyStore(Dictionary<string, RegionalBalancePolicy> items, List<string> order) : IRegionalBalancePolicyStore
{
    public Dictionary<string, RegionalBalancePolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalBalancePolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalBalancePolicy item)
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

    public List<RegionalBalancePolicy> List()
    {
        var outValue = new List<RegionalBalancePolicy>();
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
public sealed class CachedRegionalBalancePolicyStore(MemoryRegionalBalancePolicyStore inner, RegionalBalancePolicyCache cache, int hits, int misses) : IRegionalBalancePolicyStore
{
    public MemoryRegionalBalancePolicyStore Inner { get; set; } = inner;
    public RegionalBalancePolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalBalancePolicy? Find(string code)
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

    public void Save(RegionalBalancePolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalBalancePolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional balance policies. Empty fields match all.</summary>
public sealed record RegionalBalancePolicyQuery(string Department, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalBalancePolicy item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalBalancePolicy> Apply(List<RegionalBalancePolicy> items)
    {
        var matched = Collections.Filter(items, (RegionalBalancePolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional balance policy store.</summary>
public static class RegionalBalancePolicyStore
{
    public static IRegionalBalancePolicyStore NewMemoryRegionalBalancePolicyStore()
    {
        return new MemoryRegionalBalancePolicyStore(
            new Dictionary<string, RegionalBalancePolicy>(),
            new List<string>());
    }

    public static IRegionalBalancePolicyStore NewCachedRegionalBalancePolicyStore(MemoryRegionalBalancePolicyStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalBalancePolicyStore(inner, RegionalBalancePolicyCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<RegionalBalancePolicy> FindRegionalBalancePolicies(IRegionalBalancePolicyStore store, RegionalBalancePolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
