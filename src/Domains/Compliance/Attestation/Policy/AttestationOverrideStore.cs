namespace Acme.Workforce.Compliance.Attestation.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps attestation overrides by code.</summary>
public interface IAttestationOverrideStore
{
    AttestationOverride? Find(string code);
    void Save(AttestationOverride item);
    bool Remove(string code);
    List<AttestationOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAttestationOverrideStore(Dictionary<string, AttestationOverride> items, List<string> order) : IAttestationOverrideStore
{
    public Dictionary<string, AttestationOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AttestationOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AttestationOverride item)
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

    public List<AttestationOverride> List()
    {
        var outValue = new List<AttestationOverride>();
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
public sealed class CachedAttestationOverrideStore(MemoryAttestationOverrideStore inner, AttestationOverrideCache cache, int hits, int misses) : IAttestationOverrideStore
{
    public MemoryAttestationOverrideStore Inner { get; set; } = inner;
    public AttestationOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AttestationOverride? Find(string code)
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

    public void Save(AttestationOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AttestationOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects attestation overrides. Empty fields match all.</summary>
public sealed record AttestationOverrideQuery(string Label, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(AttestationOverride item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AttestationOverride> Apply(List<AttestationOverride> items)
    {
        var matched = Collections.Filter(items, (AttestationOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of attestation override store.</summary>
public static class AttestationOverrideStore
{
    public static IAttestationOverrideStore NewMemoryAttestationOverrideStore()
    {
        return new MemoryAttestationOverrideStore(new Dictionary<string, AttestationOverride>(), new List<string>());
    }

    public static IAttestationOverrideStore NewCachedAttestationOverrideStore(MemoryAttestationOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedAttestationOverrideStore(inner, AttestationOverrideCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<AttestationOverride> FindAttestationOverrides(IAttestationOverrideStore store, AttestationOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
