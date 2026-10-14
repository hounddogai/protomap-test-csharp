namespace Acme.Workforce.Compliance.Attestation.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps attestation tiers by code.</summary>
public interface IAttestationTierStore
{
    AttestationTier? Find(string code);
    void Save(AttestationTier item);
    bool Remove(string code);
    List<AttestationTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAttestationTierStore(Dictionary<string, AttestationTier> items, List<string> order) : IAttestationTierStore
{
    public Dictionary<string, AttestationTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AttestationTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AttestationTier item)
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

    public List<AttestationTier> List()
    {
        var outValue = new List<AttestationTier>();
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
public sealed class CachedAttestationTierStore(MemoryAttestationTierStore inner, AttestationTierCache cache, int hits, int misses) : IAttestationTierStore
{
    public MemoryAttestationTierStore Inner { get; set; } = inner;
    public AttestationTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AttestationTier? Find(string code)
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

    public void Save(AttestationTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AttestationTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects attestation tiers. Empty fields match all.</summary>
public sealed record AttestationTierQuery(string Label, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(AttestationTier item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AttestationTier> Apply(List<AttestationTier> items)
    {
        var matched = Collections.Filter(items, (AttestationTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of attestation tier store.</summary>
public static class AttestationTierStore
{
    public static IAttestationTierStore NewMemoryAttestationTierStore()
    {
        return new MemoryAttestationTierStore(new Dictionary<string, AttestationTier>(), new List<string>());
    }

    public static IAttestationTierStore NewCachedAttestationTierStore(MemoryAttestationTierStore inner, int ttl, IClock clock)
    {
        return new CachedAttestationTierStore(inner, AttestationTierCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<AttestationTier> FindAttestationTiers(IAttestationTierStore store, AttestationTierQuery query)
    {
        return query.Apply(store.List());
    }
}
