namespace Acme.Workforce.Talent.SuccessionPlan.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps succession plan agreements by code.</summary>
public interface ISuccessionPlanAgreementStore
{
    SuccessionPlanAgreement? Find(string code);
    void Save(SuccessionPlanAgreement item);
    bool Remove(string code);
    List<SuccessionPlanAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySuccessionPlanAgreementStore(Dictionary<string, SuccessionPlanAgreement> items, List<string> order) : ISuccessionPlanAgreementStore
{
    public Dictionary<string, SuccessionPlanAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SuccessionPlanAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SuccessionPlanAgreement item)
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

    public List<SuccessionPlanAgreement> List()
    {
        var outValue = new List<SuccessionPlanAgreement>();
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
public sealed class CachedSuccessionPlanAgreementStore(MemorySuccessionPlanAgreementStore inner, SuccessionPlanAgreementCache cache, int hits, int misses) : ISuccessionPlanAgreementStore
{
    public MemorySuccessionPlanAgreementStore Inner { get; set; } = inner;
    public SuccessionPlanAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SuccessionPlanAgreement? Find(string code)
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

    public void Save(SuccessionPlanAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SuccessionPlanAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects succession plan agreements. Empty fields match all.</summary>
public sealed record SuccessionPlanAgreementQuery(string Country, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(SuccessionPlanAgreement item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SuccessionPlanAgreement> Apply(List<SuccessionPlanAgreement> items)
    {
        var matched = Collections.Filter(items, (SuccessionPlanAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of succession plan agreement store.</summary>
public static class SuccessionPlanAgreementStore
{
    public static ISuccessionPlanAgreementStore NewMemorySuccessionPlanAgreementStore()
    {
        return new MemorySuccessionPlanAgreementStore(
            new Dictionary<string, SuccessionPlanAgreement>(),
            new List<string>());
    }

    public static ISuccessionPlanAgreementStore NewCachedSuccessionPlanAgreementStore(MemorySuccessionPlanAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedSuccessionPlanAgreementStore(
            inner,
            SuccessionPlanAgreementCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<SuccessionPlanAgreement> FindSuccessionPlanAgreements(ISuccessionPlanAgreementStore store, SuccessionPlanAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
