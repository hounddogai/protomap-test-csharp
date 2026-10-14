namespace Acme.Workforce.Compliance.Attestation.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default attestation claims by code.</summary>
public interface IDefaultAttestationClaimStore
{
    DefaultAttestationClaim? Find(string code);
    void Save(DefaultAttestationClaim item);
    bool Remove(string code);
    List<DefaultAttestationClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultAttestationClaimStore(Dictionary<string, DefaultAttestationClaim> items, List<string> order) : IDefaultAttestationClaimStore
{
    public Dictionary<string, DefaultAttestationClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultAttestationClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultAttestationClaim item)
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

    public List<DefaultAttestationClaim> List()
    {
        var outValue = new List<DefaultAttestationClaim>();
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
public sealed class CachedDefaultAttestationClaimStore(MemoryDefaultAttestationClaimStore inner, DefaultAttestationClaimCache cache, int hits, int misses) : IDefaultAttestationClaimStore
{
    public MemoryDefaultAttestationClaimStore Inner { get; set; } = inner;
    public DefaultAttestationClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultAttestationClaim? Find(string code)
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

    public void Save(DefaultAttestationClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultAttestationClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default attestation claims. Empty fields match all.</summary>
public sealed record DefaultAttestationClaimQuery(string Name, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultAttestationClaim item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultAttestationClaim> Apply(List<DefaultAttestationClaim> items)
    {
        var matched = Collections.Filter(items, (DefaultAttestationClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default attestation claim store.</summary>
public static class DefaultAttestationClaimStore
{
    public static IDefaultAttestationClaimStore NewMemoryDefaultAttestationClaimStore()
    {
        return new MemoryDefaultAttestationClaimStore(
            new Dictionary<string, DefaultAttestationClaim>(),
            new List<string>());
    }

    public static IDefaultAttestationClaimStore NewCachedDefaultAttestationClaimStore(MemoryDefaultAttestationClaimStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultAttestationClaimStore(
            inner,
            DefaultAttestationClaimCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<DefaultAttestationClaim> FindDefaultAttestationClaims(IDefaultAttestationClaimStore store, DefaultAttestationClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
