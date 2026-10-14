namespace Acme.Workforce.Absence.Balance.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps balance claims by code.</summary>
public interface IBalanceClaimStore
{
    BalanceClaim? Find(string code);
    void Save(BalanceClaim item);
    bool Remove(string code);
    List<BalanceClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBalanceClaimStore(Dictionary<string, BalanceClaim> items, List<string> order) : IBalanceClaimStore
{
    public Dictionary<string, BalanceClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BalanceClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BalanceClaim item)
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

    public List<BalanceClaim> List()
    {
        var outValue = new List<BalanceClaim>();
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
public sealed class CachedBalanceClaimStore(MemoryBalanceClaimStore inner, BalanceClaimCache cache, int hits, int misses) : IBalanceClaimStore
{
    public MemoryBalanceClaimStore Inner { get; set; } = inner;
    public BalanceClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BalanceClaim? Find(string code)
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

    public void Save(BalanceClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BalanceClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects balance claims. Empty fields match all.</summary>
public sealed record BalanceClaimQuery(string Name, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(BalanceClaim item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BalanceClaim> Apply(List<BalanceClaim> items)
    {
        var matched = Collections.Filter(items, (BalanceClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of balance claim store.</summary>
public static class BalanceClaimStore
{
    public static IBalanceClaimStore NewMemoryBalanceClaimStore()
    {
        return new MemoryBalanceClaimStore(new Dictionary<string, BalanceClaim>(), new List<string>());
    }

    public static IBalanceClaimStore NewCachedBalanceClaimStore(MemoryBalanceClaimStore inner, int ttl, IClock clock)
    {
        return new CachedBalanceClaimStore(inner, BalanceClaimCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<BalanceClaim> FindBalanceClaims(IBalanceClaimStore store, BalanceClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
