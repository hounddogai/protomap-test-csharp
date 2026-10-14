namespace Acme.Workforce.Absence.Balance.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps balance targets by code.</summary>
public interface IBalanceTargetStore
{
    BalanceTarget? Find(string code);
    void Save(BalanceTarget item);
    bool Remove(string code);
    List<BalanceTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBalanceTargetStore(Dictionary<string, BalanceTarget> items, List<string> order) : IBalanceTargetStore
{
    public Dictionary<string, BalanceTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BalanceTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BalanceTarget item)
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

    public List<BalanceTarget> List()
    {
        var outValue = new List<BalanceTarget>();
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
public sealed class CachedBalanceTargetStore(MemoryBalanceTargetStore inner, BalanceTargetCache cache, int hits, int misses) : IBalanceTargetStore
{
    public MemoryBalanceTargetStore Inner { get; set; } = inner;
    public BalanceTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BalanceTarget? Find(string code)
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

    public void Save(BalanceTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BalanceTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects balance targets. Empty fields match all.</summary>
public sealed record BalanceTargetQuery(string Unit, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(BalanceTarget item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BalanceTarget> Apply(List<BalanceTarget> items)
    {
        var matched = Collections.Filter(items, (BalanceTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of balance target store.</summary>
public static class BalanceTargetStore
{
    public static IBalanceTargetStore NewMemoryBalanceTargetStore()
    {
        return new MemoryBalanceTargetStore(new Dictionary<string, BalanceTarget>(), new List<string>());
    }

    public static IBalanceTargetStore NewCachedBalanceTargetStore(MemoryBalanceTargetStore inner, int ttl, IClock clock)
    {
        return new CachedBalanceTargetStore(inner, BalanceTargetCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<BalanceTarget> FindBalanceTargets(IBalanceTargetStore store, BalanceTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
