namespace Acme.Workforce.Orgdesign.Reorg.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional reorg policies by code.</summary>
public interface IRegionalReorgPolicyStore
{
    RegionalReorgPolicy? Find(string code);
    void Save(RegionalReorgPolicy item);
    bool Remove(string code);
    List<RegionalReorgPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalReorgPolicyStore(Dictionary<string, RegionalReorgPolicy> items, List<string> order) : IRegionalReorgPolicyStore
{
    public Dictionary<string, RegionalReorgPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalReorgPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalReorgPolicy item)
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

    public List<RegionalReorgPolicy> List()
    {
        var outValue = new List<RegionalReorgPolicy>();
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
public sealed class CachedRegionalReorgPolicyStore(MemoryRegionalReorgPolicyStore inner, RegionalReorgPolicyCache cache, int hits, int misses) : IRegionalReorgPolicyStore
{
    public MemoryRegionalReorgPolicyStore Inner { get; set; } = inner;
    public RegionalReorgPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalReorgPolicy? Find(string code)
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

    public void Save(RegionalReorgPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalReorgPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional reorg policies. Empty fields match all.</summary>
public sealed record RegionalReorgPolicyQuery(string Name, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalReorgPolicy item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<RegionalReorgPolicy> Apply(List<RegionalReorgPolicy> items)
    {
        var matched = Collections.Filter(items, (RegionalReorgPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional reorg policy store.</summary>
public static class RegionalReorgPolicyStore
{
    public static IRegionalReorgPolicyStore NewMemoryRegionalReorgPolicyStore()
    {
        return new MemoryRegionalReorgPolicyStore(new Dictionary<string, RegionalReorgPolicy>(), new List<string>());
    }

    public static IRegionalReorgPolicyStore NewCachedRegionalReorgPolicyStore(MemoryRegionalReorgPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalReorgPolicyStore(inner, RegionalReorgPolicyCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<RegionalReorgPolicy> FindRegionalReorgPolicies(IRegionalReorgPolicyStore store, RegionalReorgPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
