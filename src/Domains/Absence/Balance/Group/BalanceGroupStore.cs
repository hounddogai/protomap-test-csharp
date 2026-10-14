namespace Acme.Workforce.Absence.Balance.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps balance groups by code.</summary>
public interface IBalanceGroupStore
{
    BalanceGroup? Find(string code);
    void Save(BalanceGroup item);
    bool Remove(string code);
    List<BalanceGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBalanceGroupStore(Dictionary<string, BalanceGroup> items, List<string> order) : IBalanceGroupStore
{
    public Dictionary<string, BalanceGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BalanceGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BalanceGroup item)
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

    public List<BalanceGroup> List()
    {
        var outValue = new List<BalanceGroup>();
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
public sealed class CachedBalanceGroupStore(MemoryBalanceGroupStore inner, BalanceGroupCache cache, int hits, int misses) : IBalanceGroupStore
{
    public MemoryBalanceGroupStore Inner { get; set; } = inner;
    public BalanceGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BalanceGroup? Find(string code)
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

    public void Save(BalanceGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BalanceGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects balance groups. Empty fields match all.</summary>
public sealed record BalanceGroupQuery(string Locale, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(BalanceGroup item)
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

    public List<BalanceGroup> Apply(List<BalanceGroup> items)
    {
        var matched = Collections.Filter(items, (BalanceGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of balance group store.</summary>
public static class BalanceGroupStore
{
    public static IBalanceGroupStore NewMemoryBalanceGroupStore()
    {
        return new MemoryBalanceGroupStore(new Dictionary<string, BalanceGroup>(), new List<string>());
    }

    public static IBalanceGroupStore NewCachedBalanceGroupStore(MemoryBalanceGroupStore inner, int ttl, IClock clock)
    {
        return new CachedBalanceGroupStore(inner, BalanceGroupCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<BalanceGroup> FindBalanceGroups(IBalanceGroupStore store, BalanceGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
