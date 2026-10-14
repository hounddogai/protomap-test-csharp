namespace Acme.Workforce.Compliance.Certification.CertificationForecast;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual certification plans by code.</summary>
public interface IManualCertificationPlanStore
{
    ManualCertificationPlan? Find(string code);
    void Save(ManualCertificationPlan item);
    bool Remove(string code);
    List<ManualCertificationPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualCertificationPlanStore(Dictionary<string, ManualCertificationPlan> items, List<string> order) : IManualCertificationPlanStore
{
    public Dictionary<string, ManualCertificationPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualCertificationPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualCertificationPlan item)
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

    public List<ManualCertificationPlan> List()
    {
        var outValue = new List<ManualCertificationPlan>();
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
public sealed class CachedManualCertificationPlanStore(MemoryManualCertificationPlanStore inner, ManualCertificationPlanCache cache, int hits, int misses) : IManualCertificationPlanStore
{
    public MemoryManualCertificationPlanStore Inner { get; set; } = inner;
    public ManualCertificationPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualCertificationPlan? Find(string code)
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

    public void Save(ManualCertificationPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualCertificationPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual certification plans. Empty fields match all.</summary>
public sealed record ManualCertificationPlanQuery(string Locale, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(ManualCertificationPlan item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
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

    public List<ManualCertificationPlan> Apply(List<ManualCertificationPlan> items)
    {
        var matched = Collections.Filter(items, (ManualCertificationPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual certification plan store.</summary>
public static class ManualCertificationPlanStore
{
    public static IManualCertificationPlanStore NewMemoryManualCertificationPlanStore()
    {
        return new MemoryManualCertificationPlanStore(
            new Dictionary<string, ManualCertificationPlan>(),
            new List<string>());
    }

    public static IManualCertificationPlanStore NewCachedManualCertificationPlanStore(MemoryManualCertificationPlanStore inner, int ttl, IClock clock)
    {
        return new CachedManualCertificationPlanStore(
            inner,
            ManualCertificationPlanCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<ManualCertificationPlan> FindManualCertificationPlans(IManualCertificationPlanStore store, ManualCertificationPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
