namespace Acme.Workforce.Compliance.Certification.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps certification plans by code.</summary>
public interface ICertificationPlanStore
{
    CertificationPlan? Find(string code);
    void Save(CertificationPlan item);
    bool Remove(string code);
    List<CertificationPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCertificationPlanStore(Dictionary<string, CertificationPlan> items, List<string> order) : ICertificationPlanStore
{
    public Dictionary<string, CertificationPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CertificationPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CertificationPlan item)
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

    public List<CertificationPlan> List()
    {
        var outValue = new List<CertificationPlan>();
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
public sealed class CachedCertificationPlanStore(MemoryCertificationPlanStore inner, CertificationPlanCache cache, int hits, int misses) : ICertificationPlanStore
{
    public MemoryCertificationPlanStore Inner { get; set; } = inner;
    public CertificationPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CertificationPlan? Find(string code)
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

    public void Save(CertificationPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CertificationPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects certification plans. Empty fields match all.</summary>
public sealed record CertificationPlanQuery(string Channel, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(CertificationPlan item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CertificationPlan> Apply(List<CertificationPlan> items)
    {
        var matched = Collections.Filter(items, (CertificationPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of certification plan store.</summary>
public static class CertificationPlanStore
{
    public static ICertificationPlanStore NewMemoryCertificationPlanStore()
    {
        return new MemoryCertificationPlanStore(new Dictionary<string, CertificationPlan>(), new List<string>());
    }

    public static ICertificationPlanStore NewCachedCertificationPlanStore(MemoryCertificationPlanStore inner, int ttl, IClock clock)
    {
        return new CachedCertificationPlanStore(inner, CertificationPlanCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<CertificationPlan> FindCertificationPlans(ICertificationPlanStore store, CertificationPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
