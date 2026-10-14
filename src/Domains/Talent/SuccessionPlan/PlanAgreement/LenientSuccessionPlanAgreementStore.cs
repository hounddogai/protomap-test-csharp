namespace Acme.Workforce.Talent.SuccessionPlan.PlanAgreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient succession plan agreements by code.</summary>
public interface ILenientSuccessionPlanAgreementStore
{
    LenientSuccessionPlanAgreement? Find(string code);
    void Save(LenientSuccessionPlanAgreement item);
    bool Remove(string code);
    List<LenientSuccessionPlanAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientSuccessionPlanAgreementStore(Dictionary<string, LenientSuccessionPlanAgreement> items, List<string> order) : ILenientSuccessionPlanAgreementStore
{
    public Dictionary<string, LenientSuccessionPlanAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientSuccessionPlanAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientSuccessionPlanAgreement item)
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

    public List<LenientSuccessionPlanAgreement> List()
    {
        var outValue = new List<LenientSuccessionPlanAgreement>();
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
public sealed class CachedLenientSuccessionPlanAgreementStore(MemoryLenientSuccessionPlanAgreementStore inner, LenientSuccessionPlanAgreementCache cache, int hits, int misses) : ILenientSuccessionPlanAgreementStore
{
    public MemoryLenientSuccessionPlanAgreementStore Inner { get; set; } = inner;
    public LenientSuccessionPlanAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientSuccessionPlanAgreement? Find(string code)
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

    public void Save(LenientSuccessionPlanAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientSuccessionPlanAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient succession plan agreements. Empty fields match all.</summary>
public sealed record LenientSuccessionPlanAgreementQuery(string Name, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(LenientSuccessionPlanAgreement item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<LenientSuccessionPlanAgreement> Apply(List<LenientSuccessionPlanAgreement> items)
    {
        var matched = Collections.Filter(items, (LenientSuccessionPlanAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient succession plan agreement store.</summary>
public static class LenientSuccessionPlanAgreementStore
{
    public static ILenientSuccessionPlanAgreementStore NewMemoryLenientSuccessionPlanAgreementStore()
    {
        return new MemoryLenientSuccessionPlanAgreementStore(
            new Dictionary<string, LenientSuccessionPlanAgreement>(),
            new List<string>());
    }

    public static ILenientSuccessionPlanAgreementStore NewCachedLenientSuccessionPlanAgreementStore(MemoryLenientSuccessionPlanAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedLenientSuccessionPlanAgreementStore(
            inner,
            LenientSuccessionPlanAgreementCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<LenientSuccessionPlanAgreement> FindLenientSuccessionPlanAgreements(ILenientSuccessionPlanAgreementStore store, LenientSuccessionPlanAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
