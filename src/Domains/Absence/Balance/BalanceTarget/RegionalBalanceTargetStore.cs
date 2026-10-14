namespace Acme.Workforce.Absence.Balance.BalanceTarget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional balance targets by code.</summary>
public interface IRegionalBalanceTargetStore
{
    RegionalBalanceTarget? Find(string code);
    void Save(RegionalBalanceTarget item);
    bool Remove(string code);
    List<RegionalBalanceTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalBalanceTargetStore(Dictionary<string, RegionalBalanceTarget> items, List<string> order) : IRegionalBalanceTargetStore
{
    public Dictionary<string, RegionalBalanceTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalBalanceTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalBalanceTarget item)
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

    public List<RegionalBalanceTarget> List()
    {
        var outValue = new List<RegionalBalanceTarget>();
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
public sealed class CachedRegionalBalanceTargetStore(MemoryRegionalBalanceTargetStore inner, RegionalBalanceTargetCache cache, int hits, int misses) : IRegionalBalanceTargetStore
{
    public MemoryRegionalBalanceTargetStore Inner { get; set; } = inner;
    public RegionalBalanceTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalBalanceTarget? Find(string code)
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

    public void Save(RegionalBalanceTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalBalanceTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional balance targets. Empty fields match all.</summary>
public sealed record RegionalBalanceTargetQuery(string Unit, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalBalanceTarget item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
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

    public List<RegionalBalanceTarget> Apply(List<RegionalBalanceTarget> items)
    {
        var matched = Collections.Filter(items, (RegionalBalanceTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional balance target store.</summary>
public static class RegionalBalanceTargetStore
{
    public static IRegionalBalanceTargetStore NewMemoryRegionalBalanceTargetStore()
    {
        return new MemoryRegionalBalanceTargetStore(
            new Dictionary<string, RegionalBalanceTarget>(),
            new List<string>());
    }

    public static IRegionalBalanceTargetStore NewCachedRegionalBalanceTargetStore(MemoryRegionalBalanceTargetStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalBalanceTargetStore(
            inner,
            RegionalBalanceTargetCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<RegionalBalanceTarget> FindRegionalBalanceTargets(IRegionalBalanceTargetStore store, RegionalBalanceTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
