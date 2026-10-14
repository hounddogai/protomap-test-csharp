namespace Acme.Workforce.Compliance.Certification.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps certification targets by code.</summary>
public interface ICertificationTargetStore
{
    CertificationTarget? Find(string code);
    void Save(CertificationTarget item);
    bool Remove(string code);
    List<CertificationTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCertificationTargetStore(Dictionary<string, CertificationTarget> items, List<string> order) : ICertificationTargetStore
{
    public Dictionary<string, CertificationTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CertificationTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CertificationTarget item)
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

    public List<CertificationTarget> List()
    {
        var outValue = new List<CertificationTarget>();
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
public sealed class CachedCertificationTargetStore(MemoryCertificationTargetStore inner, CertificationTargetCache cache, int hits, int misses) : ICertificationTargetStore
{
    public MemoryCertificationTargetStore Inner { get; set; } = inner;
    public CertificationTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CertificationTarget? Find(string code)
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

    public void Save(CertificationTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CertificationTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects certification targets. Empty fields match all.</summary>
public sealed record CertificationTargetQuery(string Unit, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(CertificationTarget item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CertificationTarget> Apply(List<CertificationTarget> items)
    {
        var matched = Collections.Filter(items, (CertificationTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of certification target store.</summary>
public static class CertificationTargetStore
{
    public static ICertificationTargetStore NewMemoryCertificationTargetStore()
    {
        return new MemoryCertificationTargetStore(new Dictionary<string, CertificationTarget>(), new List<string>());
    }

    public static ICertificationTargetStore NewCachedCertificationTargetStore(MemoryCertificationTargetStore inner, int ttl, IClock clock)
    {
        return new CachedCertificationTargetStore(inner, CertificationTargetCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<CertificationTarget> FindCertificationTargets(ICertificationTargetStore store, CertificationTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
