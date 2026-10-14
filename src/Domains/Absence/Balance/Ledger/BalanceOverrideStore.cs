namespace Acme.Workforce.Absence.Balance.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps balance overrides by code.</summary>
public interface IBalanceOverrideStore
{
    BalanceOverride? Find(string code);
    void Save(BalanceOverride item);
    bool Remove(string code);
    List<BalanceOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBalanceOverrideStore(Dictionary<string, BalanceOverride> items, List<string> order) : IBalanceOverrideStore
{
    public Dictionary<string, BalanceOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BalanceOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BalanceOverride item)
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

    public List<BalanceOverride> List()
    {
        var outValue = new List<BalanceOverride>();
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
public sealed class CachedBalanceOverrideStore(MemoryBalanceOverrideStore inner, BalanceOverrideCache cache, int hits, int misses) : IBalanceOverrideStore
{
    public MemoryBalanceOverrideStore Inner { get; set; } = inner;
    public BalanceOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BalanceOverride? Find(string code)
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

    public void Save(BalanceOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BalanceOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects balance overrides. Empty fields match all.</summary>
public sealed record BalanceOverrideQuery(string Locale, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(BalanceOverride item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BalanceOverride> Apply(List<BalanceOverride> items)
    {
        var matched = Collections.Filter(items, (BalanceOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of balance override store.</summary>
public static class BalanceOverrideStore
{
    public static IBalanceOverrideStore NewMemoryBalanceOverrideStore()
    {
        return new MemoryBalanceOverrideStore(new Dictionary<string, BalanceOverride>(), new List<string>());
    }

    public static IBalanceOverrideStore NewCachedBalanceOverrideStore(MemoryBalanceOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedBalanceOverrideStore(inner, BalanceOverrideCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<BalanceOverride> FindBalanceOverrides(IBalanceOverrideStore store, BalanceOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
