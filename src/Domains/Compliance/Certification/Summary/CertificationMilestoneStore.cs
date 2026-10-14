namespace Acme.Workforce.Compliance.Certification.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps certification milestones by code.</summary>
public interface ICertificationMilestoneStore
{
    CertificationMilestone? Find(string code);
    void Save(CertificationMilestone item);
    bool Remove(string code);
    List<CertificationMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCertificationMilestoneStore(Dictionary<string, CertificationMilestone> items, List<string> order) : ICertificationMilestoneStore
{
    public Dictionary<string, CertificationMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CertificationMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CertificationMilestone item)
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

    public List<CertificationMilestone> List()
    {
        var outValue = new List<CertificationMilestone>();
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
public sealed class CachedCertificationMilestoneStore(MemoryCertificationMilestoneStore inner, CertificationMilestoneCache cache, int hits, int misses) : ICertificationMilestoneStore
{
    public MemoryCertificationMilestoneStore Inner { get; set; } = inner;
    public CertificationMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CertificationMilestone? Find(string code)
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

    public void Save(CertificationMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CertificationMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects certification milestones. Empty fields match all.</summary>
public sealed record CertificationMilestoneQuery(string Source, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(CertificationMilestone item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CertificationMilestone> Apply(List<CertificationMilestone> items)
    {
        var matched = Collections.Filter(items, (CertificationMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of certification milestone store.</summary>
public static class CertificationMilestoneStore
{
    public static ICertificationMilestoneStore NewMemoryCertificationMilestoneStore()
    {
        return new MemoryCertificationMilestoneStore(
            new Dictionary<string, CertificationMilestone>(),
            new List<string>());
    }

    public static ICertificationMilestoneStore NewCachedCertificationMilestoneStore(MemoryCertificationMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedCertificationMilestoneStore(
            inner,
            CertificationMilestoneCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<CertificationMilestone> FindCertificationMilestones(ICertificationMilestoneStore store, CertificationMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
