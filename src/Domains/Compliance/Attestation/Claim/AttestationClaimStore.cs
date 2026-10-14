namespace Acme.Workforce.Compliance.Attestation.Claim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps attestation claims by code.</summary>
public interface IAttestationClaimStore
{
    AttestationClaim? Find(string code);
    void Save(AttestationClaim item);
    bool Remove(string code);
    List<AttestationClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAttestationClaimStore(Dictionary<string, AttestationClaim> items, List<string> order) : IAttestationClaimStore
{
    public Dictionary<string, AttestationClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AttestationClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AttestationClaim item)
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

    public List<AttestationClaim> List()
    {
        var outValue = new List<AttestationClaim>();
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
public sealed class CachedAttestationClaimStore(MemoryAttestationClaimStore inner, AttestationClaimCache cache, int hits, int misses) : IAttestationClaimStore
{
    public MemoryAttestationClaimStore Inner { get; set; } = inner;
    public AttestationClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AttestationClaim? Find(string code)
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

    public void Save(AttestationClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AttestationClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects attestation claims. Empty fields match all.</summary>
public sealed record AttestationClaimQuery(string Title, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(AttestationClaim item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AttestationClaim> Apply(List<AttestationClaim> items)
    {
        var matched = Collections.Filter(items, (AttestationClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of attestation claim store.</summary>
public static class AttestationClaimStore
{
    public static IAttestationClaimStore NewMemoryAttestationClaimStore()
    {
        return new MemoryAttestationClaimStore(new Dictionary<string, AttestationClaim>(), new List<string>());
    }

    public static IAttestationClaimStore NewCachedAttestationClaimStore(MemoryAttestationClaimStore inner, int ttl, IClock clock)
    {
        return new CachedAttestationClaimStore(inner, AttestationClaimCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<AttestationClaim> FindAttestationClaims(IAttestationClaimStore store, AttestationClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
