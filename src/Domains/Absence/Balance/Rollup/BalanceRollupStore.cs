namespace Acme.Workforce.Absence.Balance.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps balance rollups by code.</summary>
public interface IBalanceRollupStore
{
    BalanceRollup? Find(string code);
    void Save(BalanceRollup item);
    bool Remove(string code);
    List<BalanceRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBalanceRollupStore(Dictionary<string, BalanceRollup> items, List<string> order) : IBalanceRollupStore
{
    public Dictionary<string, BalanceRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BalanceRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BalanceRollup item)
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

    public List<BalanceRollup> List()
    {
        var outValue = new List<BalanceRollup>();
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
public sealed class CachedBalanceRollupStore(MemoryBalanceRollupStore inner, BalanceRollupCache cache, int hits, int misses) : IBalanceRollupStore
{
    public MemoryBalanceRollupStore Inner { get; set; } = inner;
    public BalanceRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BalanceRollup? Find(string code)
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

    public void Save(BalanceRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BalanceRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects balance rollups. Empty fields match all.</summary>
public sealed record BalanceRollupQuery(string Name, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(BalanceRollup item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BalanceRollup> Apply(List<BalanceRollup> items)
    {
        var matched = Collections.Filter(items, (BalanceRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of balance rollup store.</summary>
public static class BalanceRollupStore
{
    public static IBalanceRollupStore NewMemoryBalanceRollupStore()
    {
        return new MemoryBalanceRollupStore(new Dictionary<string, BalanceRollup>(), new List<string>());
    }

    public static IBalanceRollupStore NewCachedBalanceRollupStore(MemoryBalanceRollupStore inner, int ttl, IClock clock)
    {
        return new CachedBalanceRollupStore(inner, BalanceRollupCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<BalanceRollup> FindBalanceRollups(IBalanceRollupStore store, BalanceRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
