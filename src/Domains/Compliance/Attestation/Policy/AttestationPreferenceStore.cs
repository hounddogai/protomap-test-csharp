namespace Acme.Workforce.Compliance.Attestation.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps attestation preferences by code.</summary>
public interface IAttestationPreferenceStore
{
    AttestationPreference? Find(string code);
    void Save(AttestationPreference item);
    bool Remove(string code);
    List<AttestationPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAttestationPreferenceStore(Dictionary<string, AttestationPreference> items, List<string> order) : IAttestationPreferenceStore
{
    public Dictionary<string, AttestationPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AttestationPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AttestationPreference item)
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

    public List<AttestationPreference> List()
    {
        var outValue = new List<AttestationPreference>();
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
public sealed class CachedAttestationPreferenceStore(MemoryAttestationPreferenceStore inner, AttestationPreferenceCache cache, int hits, int misses) : IAttestationPreferenceStore
{
    public MemoryAttestationPreferenceStore Inner { get; set; } = inner;
    public AttestationPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AttestationPreference? Find(string code)
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

    public void Save(AttestationPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AttestationPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects attestation preferences. Empty fields match all.</summary>
public sealed record AttestationPreferenceQuery(string Label, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(AttestationPreference item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AttestationPreference> Apply(List<AttestationPreference> items)
    {
        var matched = Collections.Filter(items, (AttestationPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of attestation preference store.</summary>
public static class AttestationPreferenceStore
{
    public static IAttestationPreferenceStore NewMemoryAttestationPreferenceStore()
    {
        return new MemoryAttestationPreferenceStore(
            new Dictionary<string, AttestationPreference>(),
            new List<string>());
    }

    public static IAttestationPreferenceStore NewCachedAttestationPreferenceStore(MemoryAttestationPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedAttestationPreferenceStore(inner, AttestationPreferenceCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<AttestationPreference> FindAttestationPreferences(IAttestationPreferenceStore store, AttestationPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
