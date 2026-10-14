namespace Acme.Workforce.Talent.SuccessionPlan.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional succession plan agreements by code.</summary>
public interface IRegionalSuccessionPlanAgreementStore
{
    RegionalSuccessionPlanAgreement? Find(string code);
    void Save(RegionalSuccessionPlanAgreement item);
    bool Remove(string code);
    List<RegionalSuccessionPlanAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalSuccessionPlanAgreementStore(Dictionary<string, RegionalSuccessionPlanAgreement> items, List<string> order) : IRegionalSuccessionPlanAgreementStore
{
    public Dictionary<string, RegionalSuccessionPlanAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalSuccessionPlanAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalSuccessionPlanAgreement item)
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

    public List<RegionalSuccessionPlanAgreement> List()
    {
        var outValue = new List<RegionalSuccessionPlanAgreement>();
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
public sealed class CachedRegionalSuccessionPlanAgreementStore(MemoryRegionalSuccessionPlanAgreementStore inner, RegionalSuccessionPlanAgreementCache cache, int hits, int misses) : IRegionalSuccessionPlanAgreementStore
{
    public MemoryRegionalSuccessionPlanAgreementStore Inner { get; set; } = inner;
    public RegionalSuccessionPlanAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalSuccessionPlanAgreement? Find(string code)
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

    public void Save(RegionalSuccessionPlanAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalSuccessionPlanAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional succession plan agreements. Empty fields match all.</summary>
public sealed record RegionalSuccessionPlanAgreementQuery(string Segment, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalSuccessionPlanAgreement item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalSuccessionPlanAgreement> Apply(List<RegionalSuccessionPlanAgreement> items)
    {
        var matched = Collections.Filter(items, (RegionalSuccessionPlanAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional succession plan agreement store.</summary>
public static class RegionalSuccessionPlanAgreementStore
{
    public static IRegionalSuccessionPlanAgreementStore NewMemoryRegionalSuccessionPlanAgreementStore()
    {
        return new MemoryRegionalSuccessionPlanAgreementStore(
            new Dictionary<string, RegionalSuccessionPlanAgreement>(),
            new List<string>());
    }

    public static IRegionalSuccessionPlanAgreementStore NewCachedRegionalSuccessionPlanAgreementStore(MemoryRegionalSuccessionPlanAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalSuccessionPlanAgreementStore(
            inner,
            RegionalSuccessionPlanAgreementCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<RegionalSuccessionPlanAgreement> FindRegionalSuccessionPlanAgreements(IRegionalSuccessionPlanAgreementStore store, RegionalSuccessionPlanAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
