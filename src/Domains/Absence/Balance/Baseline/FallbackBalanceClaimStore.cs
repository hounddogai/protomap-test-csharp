namespace Acme.Workforce.Absence.Balance.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback balance claims by code.</summary>
public interface IFallbackBalanceClaimStore
{
    FallbackBalanceClaim? Find(string code);
    void Save(FallbackBalanceClaim item);
    bool Remove(string code);
    List<FallbackBalanceClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackBalanceClaimStore(Dictionary<string, FallbackBalanceClaim> items, List<string> order) : IFallbackBalanceClaimStore
{
    public Dictionary<string, FallbackBalanceClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackBalanceClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackBalanceClaim item)
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

    public List<FallbackBalanceClaim> List()
    {
        var outValue = new List<FallbackBalanceClaim>();
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
public sealed class CachedFallbackBalanceClaimStore(MemoryFallbackBalanceClaimStore inner, FallbackBalanceClaimCache cache, int hits, int misses) : IFallbackBalanceClaimStore
{
    public MemoryFallbackBalanceClaimStore Inner { get; set; } = inner;
    public FallbackBalanceClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackBalanceClaim? Find(string code)
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

    public void Save(FallbackBalanceClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackBalanceClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback balance claims. Empty fields match all.</summary>
public sealed record FallbackBalanceClaimQuery(string Unit, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackBalanceClaim item)
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

    public List<FallbackBalanceClaim> Apply(List<FallbackBalanceClaim> items)
    {
        var matched = Collections.Filter(items, (FallbackBalanceClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback balance claim store.</summary>
public static class FallbackBalanceClaimStore
{
    public static IFallbackBalanceClaimStore NewMemoryFallbackBalanceClaimStore()
    {
        return new MemoryFallbackBalanceClaimStore(
            new Dictionary<string, FallbackBalanceClaim>(),
            new List<string>());
    }

    public static IFallbackBalanceClaimStore NewCachedFallbackBalanceClaimStore(MemoryFallbackBalanceClaimStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackBalanceClaimStore(inner, FallbackBalanceClaimCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<FallbackBalanceClaim> FindFallbackBalanceClaims(IFallbackBalanceClaimStore store, FallbackBalanceClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
