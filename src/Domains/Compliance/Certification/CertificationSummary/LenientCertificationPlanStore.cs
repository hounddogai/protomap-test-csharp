namespace Acme.Workforce.Compliance.Certification.CertificationSummary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient certification plans by code.</summary>
public interface ILenientCertificationPlanStore
{
    LenientCertificationPlan? Find(string code);
    void Save(LenientCertificationPlan item);
    bool Remove(string code);
    List<LenientCertificationPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientCertificationPlanStore(Dictionary<string, LenientCertificationPlan> items, List<string> order) : ILenientCertificationPlanStore
{
    public Dictionary<string, LenientCertificationPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientCertificationPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientCertificationPlan item)
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

    public List<LenientCertificationPlan> List()
    {
        var outValue = new List<LenientCertificationPlan>();
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
public sealed class CachedLenientCertificationPlanStore(MemoryLenientCertificationPlanStore inner, LenientCertificationPlanCache cache, int hits, int misses) : ILenientCertificationPlanStore
{
    public MemoryLenientCertificationPlanStore Inner { get; set; } = inner;
    public LenientCertificationPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientCertificationPlan? Find(string code)
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

    public void Save(LenientCertificationPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientCertificationPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient certification plans. Empty fields match all.</summary>
public sealed record LenientCertificationPlanQuery(string Label, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(LenientCertificationPlan item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientCertificationPlan> Apply(List<LenientCertificationPlan> items)
    {
        var matched = Collections.Filter(items, (LenientCertificationPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient certification plan store.</summary>
public static class LenientCertificationPlanStore
{
    public static ILenientCertificationPlanStore NewMemoryLenientCertificationPlanStore()
    {
        return new MemoryLenientCertificationPlanStore(
            new Dictionary<string, LenientCertificationPlan>(),
            new List<string>());
    }

    public static ILenientCertificationPlanStore NewCachedLenientCertificationPlanStore(MemoryLenientCertificationPlanStore inner, int ttl, IClock clock)
    {
        return new CachedLenientCertificationPlanStore(
            inner,
            LenientCertificationPlanCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<LenientCertificationPlan> FindLenientCertificationPlans(ILenientCertificationPlanStore store, LenientCertificationPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
