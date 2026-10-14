namespace Acme.Workforce.Orgdesign.Position.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional position policies by code.</summary>
public interface IRegionalPositionPolicyStore
{
    RegionalPositionPolicy? Find(string code);
    void Save(RegionalPositionPolicy item);
    bool Remove(string code);
    List<RegionalPositionPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalPositionPolicyStore(Dictionary<string, RegionalPositionPolicy> items, List<string> order) : IRegionalPositionPolicyStore
{
    public Dictionary<string, RegionalPositionPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalPositionPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalPositionPolicy item)
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

    public List<RegionalPositionPolicy> List()
    {
        var outValue = new List<RegionalPositionPolicy>();
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
public sealed class CachedRegionalPositionPolicyStore(MemoryRegionalPositionPolicyStore inner, RegionalPositionPolicyCache cache, int hits, int misses) : IRegionalPositionPolicyStore
{
    public MemoryRegionalPositionPolicyStore Inner { get; set; } = inner;
    public RegionalPositionPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalPositionPolicy? Find(string code)
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

    public void Save(RegionalPositionPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalPositionPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional position policies. Empty fields match all.</summary>
public sealed record RegionalPositionPolicyQuery(string Label, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalPositionPolicy item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<RegionalPositionPolicy> Apply(List<RegionalPositionPolicy> items)
    {
        var matched = Collections.Filter(items, (RegionalPositionPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional position policy store.</summary>
public static class RegionalPositionPolicyStore
{
    public static IRegionalPositionPolicyStore NewMemoryRegionalPositionPolicyStore()
    {
        return new MemoryRegionalPositionPolicyStore(
            new Dictionary<string, RegionalPositionPolicy>(),
            new List<string>());
    }

    public static IRegionalPositionPolicyStore NewCachedRegionalPositionPolicyStore(MemoryRegionalPositionPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalPositionPolicyStore(
            inner,
            RegionalPositionPolicyCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<RegionalPositionPolicy> FindRegionalPositionPolicies(IRegionalPositionPolicyStore store, RegionalPositionPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
