namespace Acme.Workforce.Compliance.Attestation.Digest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted attestation claims by code.</summary>
public interface IWeightedAttestationClaimStore
{
    WeightedAttestationClaim? Find(string code);
    void Save(WeightedAttestationClaim item);
    bool Remove(string code);
    List<WeightedAttestationClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedAttestationClaimStore(Dictionary<string, WeightedAttestationClaim> items, List<string> order) : IWeightedAttestationClaimStore
{
    public Dictionary<string, WeightedAttestationClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedAttestationClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedAttestationClaim item)
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

    public List<WeightedAttestationClaim> List()
    {
        var outValue = new List<WeightedAttestationClaim>();
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
public sealed class CachedWeightedAttestationClaimStore(MemoryWeightedAttestationClaimStore inner, WeightedAttestationClaimCache cache, int hits, int misses) : IWeightedAttestationClaimStore
{
    public MemoryWeightedAttestationClaimStore Inner { get; set; } = inner;
    public WeightedAttestationClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedAttestationClaim? Find(string code)
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

    public void Save(WeightedAttestationClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedAttestationClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted attestation claims. Empty fields match all.</summary>
public sealed record WeightedAttestationClaimQuery(string Title, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedAttestationClaim item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedAttestationClaim> Apply(List<WeightedAttestationClaim> items)
    {
        var matched = Collections.Filter(items, (WeightedAttestationClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted attestation claim store.</summary>
public static class WeightedAttestationClaimStore
{
    public static IWeightedAttestationClaimStore NewMemoryWeightedAttestationClaimStore()
    {
        return new MemoryWeightedAttestationClaimStore(
            new Dictionary<string, WeightedAttestationClaim>(),
            new List<string>());
    }

    public static IWeightedAttestationClaimStore NewCachedWeightedAttestationClaimStore(MemoryWeightedAttestationClaimStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedAttestationClaimStore(
            inner,
            WeightedAttestationClaimCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<WeightedAttestationClaim> FindWeightedAttestationClaims(IWeightedAttestationClaimStore store, WeightedAttestationClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
