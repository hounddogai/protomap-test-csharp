namespace Acme.Workforce.Compliance.Attestation.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps attestation agreements by code.</summary>
public interface IAttestationAgreementStore
{
    AttestationAgreement? Find(string code);
    void Save(AttestationAgreement item);
    bool Remove(string code);
    List<AttestationAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAttestationAgreementStore(Dictionary<string, AttestationAgreement> items, List<string> order) : IAttestationAgreementStore
{
    public Dictionary<string, AttestationAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AttestationAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AttestationAgreement item)
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

    public List<AttestationAgreement> List()
    {
        var outValue = new List<AttestationAgreement>();
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
public sealed class CachedAttestationAgreementStore(MemoryAttestationAgreementStore inner, AttestationAgreementCache cache, int hits, int misses) : IAttestationAgreementStore
{
    public MemoryAttestationAgreementStore Inner { get; set; } = inner;
    public AttestationAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AttestationAgreement? Find(string code)
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

    public void Save(AttestationAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AttestationAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects attestation agreements. Empty fields match all.</summary>
public sealed record AttestationAgreementQuery(string Category, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(AttestationAgreement item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AttestationAgreement> Apply(List<AttestationAgreement> items)
    {
        var matched = Collections.Filter(items, (AttestationAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of attestation agreement store.</summary>
public static class AttestationAgreementStore
{
    public static IAttestationAgreementStore NewMemoryAttestationAgreementStore()
    {
        return new MemoryAttestationAgreementStore(
            new Dictionary<string, AttestationAgreement>(),
            new List<string>());
    }

    public static IAttestationAgreementStore NewCachedAttestationAgreementStore(MemoryAttestationAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedAttestationAgreementStore(inner, AttestationAgreementCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<AttestationAgreement> FindAttestationAgreements(IAttestationAgreementStore store, AttestationAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
