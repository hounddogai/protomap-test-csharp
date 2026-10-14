namespace Acme.Workforce.Payrollops.RetroPay.Claim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted retro pay claims by code.</summary>
public interface IWeightedRetroPayClaimStore
{
    WeightedRetroPayClaim? Find(string code);
    void Save(WeightedRetroPayClaim item);
    bool Remove(string code);
    List<WeightedRetroPayClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedRetroPayClaimStore(Dictionary<string, WeightedRetroPayClaim> items, List<string> order) : IWeightedRetroPayClaimStore
{
    public Dictionary<string, WeightedRetroPayClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedRetroPayClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedRetroPayClaim item)
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

    public List<WeightedRetroPayClaim> List()
    {
        var outValue = new List<WeightedRetroPayClaim>();
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
public sealed class CachedWeightedRetroPayClaimStore(MemoryWeightedRetroPayClaimStore inner, WeightedRetroPayClaimCache cache, int hits, int misses) : IWeightedRetroPayClaimStore
{
    public MemoryWeightedRetroPayClaimStore Inner { get; set; } = inner;
    public WeightedRetroPayClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedRetroPayClaim? Find(string code)
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

    public void Save(WeightedRetroPayClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedRetroPayClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted retro pay claims. Empty fields match all.</summary>
public sealed record WeightedRetroPayClaimQuery(string Locale, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedRetroPayClaim item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedRetroPayClaim> Apply(List<WeightedRetroPayClaim> items)
    {
        var matched = Collections.Filter(items, (WeightedRetroPayClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted retro pay claim store.</summary>
public static class WeightedRetroPayClaimStore
{
    public static IWeightedRetroPayClaimStore NewMemoryWeightedRetroPayClaimStore()
    {
        return new MemoryWeightedRetroPayClaimStore(
            new Dictionary<string, WeightedRetroPayClaim>(),
            new List<string>());
    }

    public static IWeightedRetroPayClaimStore NewCachedWeightedRetroPayClaimStore(MemoryWeightedRetroPayClaimStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedRetroPayClaimStore(inner, WeightedRetroPayClaimCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<WeightedRetroPayClaim> FindWeightedRetroPayClaims(IWeightedRetroPayClaimStore store, WeightedRetroPayClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
