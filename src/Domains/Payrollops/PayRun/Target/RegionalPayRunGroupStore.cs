namespace Acme.Workforce.Payrollops.PayRun.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional pay run groups by code.</summary>
public interface IRegionalPayRunGroupStore
{
    RegionalPayRunGroup? Find(string code);
    void Save(RegionalPayRunGroup item);
    bool Remove(string code);
    List<RegionalPayRunGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalPayRunGroupStore(Dictionary<string, RegionalPayRunGroup> items, List<string> order) : IRegionalPayRunGroupStore
{
    public Dictionary<string, RegionalPayRunGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalPayRunGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalPayRunGroup item)
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

    public List<RegionalPayRunGroup> List()
    {
        var outValue = new List<RegionalPayRunGroup>();
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
public sealed class CachedRegionalPayRunGroupStore(MemoryRegionalPayRunGroupStore inner, RegionalPayRunGroupCache cache, int hits, int misses) : IRegionalPayRunGroupStore
{
    public MemoryRegionalPayRunGroupStore Inner { get; set; } = inner;
    public RegionalPayRunGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalPayRunGroup? Find(string code)
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

    public void Save(RegionalPayRunGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalPayRunGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional pay run groups. Empty fields match all.</summary>
public sealed record RegionalPayRunGroupQuery(string Name, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalPayRunGroup item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalPayRunGroup> Apply(List<RegionalPayRunGroup> items)
    {
        var matched = Collections.Filter(items, (RegionalPayRunGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional pay run group store.</summary>
public static class RegionalPayRunGroupStore
{
    public static IRegionalPayRunGroupStore NewMemoryRegionalPayRunGroupStore()
    {
        return new MemoryRegionalPayRunGroupStore(new Dictionary<string, RegionalPayRunGroup>(), new List<string>());
    }

    public static IRegionalPayRunGroupStore NewCachedRegionalPayRunGroupStore(MemoryRegionalPayRunGroupStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalPayRunGroupStore(inner, RegionalPayRunGroupCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<RegionalPayRunGroup> FindRegionalPayRunGroups(IRegionalPayRunGroupStore store, RegionalPayRunGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
