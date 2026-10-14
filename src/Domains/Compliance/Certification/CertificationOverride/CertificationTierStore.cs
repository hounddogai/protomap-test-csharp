namespace Acme.Workforce.Compliance.Certification.CertificationOverride;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps certification tiers by code.</summary>
public interface ICertificationTierStore
{
    CertificationTier? Find(string code);
    void Save(CertificationTier item);
    bool Remove(string code);
    List<CertificationTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCertificationTierStore(Dictionary<string, CertificationTier> items, List<string> order) : ICertificationTierStore
{
    public Dictionary<string, CertificationTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CertificationTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CertificationTier item)
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

    public List<CertificationTier> List()
    {
        var outValue = new List<CertificationTier>();
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
public sealed class CachedCertificationTierStore(MemoryCertificationTierStore inner, CertificationTierCache cache, int hits, int misses) : ICertificationTierStore
{
    public MemoryCertificationTierStore Inner { get; set; } = inner;
    public CertificationTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CertificationTier? Find(string code)
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

    public void Save(CertificationTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CertificationTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects certification tiers. Empty fields match all.</summary>
public sealed record CertificationTierQuery(string Team, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(CertificationTier item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<CertificationTier> Apply(List<CertificationTier> items)
    {
        var matched = Collections.Filter(items, (CertificationTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of certification tier store.</summary>
public static class CertificationTierStore
{
    public static ICertificationTierStore NewMemoryCertificationTierStore()
    {
        return new MemoryCertificationTierStore(new Dictionary<string, CertificationTier>(), new List<string>());
    }

    public static ICertificationTierStore NewCachedCertificationTierStore(MemoryCertificationTierStore inner, int ttl, IClock clock)
    {
        return new CachedCertificationTierStore(inner, CertificationTierCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<CertificationTier> FindCertificationTiers(ICertificationTierStore store, CertificationTierQuery query)
    {
        return query.Apply(store.List());
    }
}
