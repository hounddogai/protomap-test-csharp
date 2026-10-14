namespace Acme.Workforce.Compliance.Certification.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps certification requests by code.</summary>
public interface ICertificationRequestStore
{
    CertificationRequest? Find(string code);
    void Save(CertificationRequest item);
    bool Remove(string code);
    List<CertificationRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCertificationRequestStore(Dictionary<string, CertificationRequest> items, List<string> order) : ICertificationRequestStore
{
    public Dictionary<string, CertificationRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CertificationRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CertificationRequest item)
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

    public List<CertificationRequest> List()
    {
        var outValue = new List<CertificationRequest>();
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
public sealed class CachedCertificationRequestStore(MemoryCertificationRequestStore inner, CertificationRequestCache cache, int hits, int misses) : ICertificationRequestStore
{
    public MemoryCertificationRequestStore Inner { get; set; } = inner;
    public CertificationRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CertificationRequest? Find(string code)
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

    public void Save(CertificationRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CertificationRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects certification requests. Empty fields match all.</summary>
public sealed record CertificationRequestQuery(string Title, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(CertificationRequest item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<CertificationRequest> Apply(List<CertificationRequest> items)
    {
        var matched = Collections.Filter(items, (CertificationRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of certification request store.</summary>
public static class CertificationRequestStore
{
    public static ICertificationRequestStore NewMemoryCertificationRequestStore()
    {
        return new MemoryCertificationRequestStore(
            new Dictionary<string, CertificationRequest>(),
            new List<string>());
    }

    public static ICertificationRequestStore NewCachedCertificationRequestStore(MemoryCertificationRequestStore inner, int ttl, IClock clock)
    {
        return new CachedCertificationRequestStore(inner, CertificationRequestCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<CertificationRequest> FindCertificationRequests(ICertificationRequestStore store, CertificationRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
