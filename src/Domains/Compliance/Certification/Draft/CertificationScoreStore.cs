namespace Acme.Workforce.Compliance.Certification.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps certification scores by code.</summary>
public interface ICertificationScoreStore
{
    CertificationScore? Find(string code);
    void Save(CertificationScore item);
    bool Remove(string code);
    List<CertificationScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCertificationScoreStore(Dictionary<string, CertificationScore> items, List<string> order) : ICertificationScoreStore
{
    public Dictionary<string, CertificationScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CertificationScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CertificationScore item)
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

    public List<CertificationScore> List()
    {
        var outValue = new List<CertificationScore>();
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
public sealed class CachedCertificationScoreStore(MemoryCertificationScoreStore inner, CertificationScoreCache cache, int hits, int misses) : ICertificationScoreStore
{
    public MemoryCertificationScoreStore Inner { get; set; } = inner;
    public CertificationScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CertificationScore? Find(string code)
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

    public void Save(CertificationScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CertificationScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects certification scores. Empty fields match all.</summary>
public sealed record CertificationScoreQuery(string Channel, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(CertificationScore item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CertificationScore> Apply(List<CertificationScore> items)
    {
        var matched = Collections.Filter(items, (CertificationScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of certification score store.</summary>
public static class CertificationScoreStore
{
    public static ICertificationScoreStore NewMemoryCertificationScoreStore()
    {
        return new MemoryCertificationScoreStore(new Dictionary<string, CertificationScore>(), new List<string>());
    }

    public static ICertificationScoreStore NewCachedCertificationScoreStore(MemoryCertificationScoreStore inner, int ttl, IClock clock)
    {
        return new CachedCertificationScoreStore(inner, CertificationScoreCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<CertificationScore> FindCertificationScores(ICertificationScoreStore store, CertificationScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
