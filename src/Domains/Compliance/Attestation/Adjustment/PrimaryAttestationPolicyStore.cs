namespace Acme.Workforce.Compliance.Attestation.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary attestation policies by code.</summary>
public interface IPrimaryAttestationPolicyStore
{
    PrimaryAttestationPolicy? Find(string code);
    void Save(PrimaryAttestationPolicy item);
    bool Remove(string code);
    List<PrimaryAttestationPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryAttestationPolicyStore(Dictionary<string, PrimaryAttestationPolicy> items, List<string> order) : IPrimaryAttestationPolicyStore
{
    public Dictionary<string, PrimaryAttestationPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryAttestationPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryAttestationPolicy item)
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

    public List<PrimaryAttestationPolicy> List()
    {
        var outValue = new List<PrimaryAttestationPolicy>();
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
public sealed class CachedPrimaryAttestationPolicyStore(MemoryPrimaryAttestationPolicyStore inner, PrimaryAttestationPolicyCache cache, int hits, int misses) : IPrimaryAttestationPolicyStore
{
    public MemoryPrimaryAttestationPolicyStore Inner { get; set; } = inner;
    public PrimaryAttestationPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryAttestationPolicy? Find(string code)
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

    public void Save(PrimaryAttestationPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryAttestationPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary attestation policies. Empty fields match all.</summary>
public sealed record PrimaryAttestationPolicyQuery(string Name, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryAttestationPolicy item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryAttestationPolicy> Apply(List<PrimaryAttestationPolicy> items)
    {
        var matched = Collections.Filter(items, (PrimaryAttestationPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary attestation policy store.</summary>
public static class PrimaryAttestationPolicyStore
{
    public static IPrimaryAttestationPolicyStore NewMemoryPrimaryAttestationPolicyStore()
    {
        return new MemoryPrimaryAttestationPolicyStore(
            new Dictionary<string, PrimaryAttestationPolicy>(),
            new List<string>());
    }

    public static IPrimaryAttestationPolicyStore NewCachedPrimaryAttestationPolicyStore(MemoryPrimaryAttestationPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryAttestationPolicyStore(
            inner,
            PrimaryAttestationPolicyCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryAttestationPolicy> FindPrimaryAttestationPolicies(IPrimaryAttestationPolicyStore store, PrimaryAttestationPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
