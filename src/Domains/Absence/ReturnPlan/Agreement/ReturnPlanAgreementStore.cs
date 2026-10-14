namespace Acme.Workforce.Absence.ReturnPlan.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps return plan agreements by code.</summary>
public interface IReturnPlanAgreementStore
{
    ReturnPlanAgreement? Find(string code);
    void Save(ReturnPlanAgreement item);
    bool Remove(string code);
    List<ReturnPlanAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReturnPlanAgreementStore(Dictionary<string, ReturnPlanAgreement> items, List<string> order) : IReturnPlanAgreementStore
{
    public Dictionary<string, ReturnPlanAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReturnPlanAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReturnPlanAgreement item)
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

    public List<ReturnPlanAgreement> List()
    {
        var outValue = new List<ReturnPlanAgreement>();
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
public sealed class CachedReturnPlanAgreementStore(MemoryReturnPlanAgreementStore inner, ReturnPlanAgreementCache cache, int hits, int misses) : IReturnPlanAgreementStore
{
    public MemoryReturnPlanAgreementStore Inner { get; set; } = inner;
    public ReturnPlanAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReturnPlanAgreement? Find(string code)
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

    public void Save(ReturnPlanAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReturnPlanAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects return plan agreements. Empty fields match all.</summary>
public sealed record ReturnPlanAgreementQuery(string Locale, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(ReturnPlanAgreement item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ReturnPlanAgreement> Apply(List<ReturnPlanAgreement> items)
    {
        var matched = Collections.Filter(items, (ReturnPlanAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of return plan agreement store.</summary>
public static class ReturnPlanAgreementStore
{
    public static IReturnPlanAgreementStore NewMemoryReturnPlanAgreementStore()
    {
        return new MemoryReturnPlanAgreementStore(new Dictionary<string, ReturnPlanAgreement>(), new List<string>());
    }

    public static IReturnPlanAgreementStore NewCachedReturnPlanAgreementStore(MemoryReturnPlanAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedReturnPlanAgreementStore(inner, ReturnPlanAgreementCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<ReturnPlanAgreement> FindReturnPlanAgreements(IReturnPlanAgreementStore store, ReturnPlanAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
