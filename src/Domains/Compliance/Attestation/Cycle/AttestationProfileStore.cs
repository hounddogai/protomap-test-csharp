namespace Acme.Workforce.Compliance.Attestation.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps attestation profiles by code.</summary>
public interface IAttestationProfileStore
{
    AttestationProfile? Find(string code);
    void Save(AttestationProfile item);
    bool Remove(string code);
    List<AttestationProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAttestationProfileStore(Dictionary<string, AttestationProfile> items, List<string> order) : IAttestationProfileStore
{
    public Dictionary<string, AttestationProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AttestationProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AttestationProfile item)
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

    public List<AttestationProfile> List()
    {
        var outValue = new List<AttestationProfile>();
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
public sealed class CachedAttestationProfileStore(MemoryAttestationProfileStore inner, AttestationProfileCache cache, int hits, int misses) : IAttestationProfileStore
{
    public MemoryAttestationProfileStore Inner { get; set; } = inner;
    public AttestationProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AttestationProfile? Find(string code)
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

    public void Save(AttestationProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AttestationProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects attestation profiles. Empty fields match all.</summary>
public sealed record AttestationProfileQuery(string Country, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(AttestationProfile item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AttestationProfile> Apply(List<AttestationProfile> items)
    {
        var matched = Collections.Filter(items, (AttestationProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of attestation profile store.</summary>
public static class AttestationProfileStore
{
    public static IAttestationProfileStore NewMemoryAttestationProfileStore()
    {
        return new MemoryAttestationProfileStore(new Dictionary<string, AttestationProfile>(), new List<string>());
    }

    public static IAttestationProfileStore NewCachedAttestationProfileStore(MemoryAttestationProfileStore inner, int ttl, IClock clock)
    {
        return new CachedAttestationProfileStore(inner, AttestationProfileCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<AttestationProfile> FindAttestationProfiles(IAttestationProfileStore store, AttestationProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
