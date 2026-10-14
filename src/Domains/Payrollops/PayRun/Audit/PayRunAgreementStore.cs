namespace Acme.Workforce.Payrollops.PayRun.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay run agreements by code.</summary>
public interface IPayRunAgreementStore
{
    PayRunAgreement? Find(string code);
    void Save(PayRunAgreement item);
    bool Remove(string code);
    List<PayRunAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayRunAgreementStore(Dictionary<string, PayRunAgreement> items, List<string> order) : IPayRunAgreementStore
{
    public Dictionary<string, PayRunAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayRunAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayRunAgreement item)
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

    public List<PayRunAgreement> List()
    {
        var outValue = new List<PayRunAgreement>();
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
public sealed class CachedPayRunAgreementStore(MemoryPayRunAgreementStore inner, PayRunAgreementCache cache, int hits, int misses) : IPayRunAgreementStore
{
    public MemoryPayRunAgreementStore Inner { get; set; } = inner;
    public PayRunAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayRunAgreement? Find(string code)
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

    public void Save(PayRunAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayRunAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay run agreements. Empty fields match all.</summary>
public sealed record PayRunAgreementQuery(string Category, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(PayRunAgreement item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayRunAgreement> Apply(List<PayRunAgreement> items)
    {
        var matched = Collections.Filter(items, (PayRunAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay run agreement store.</summary>
public static class PayRunAgreementStore
{
    public static IPayRunAgreementStore NewMemoryPayRunAgreementStore()
    {
        return new MemoryPayRunAgreementStore(new Dictionary<string, PayRunAgreement>(), new List<string>());
    }

    public static IPayRunAgreementStore NewCachedPayRunAgreementStore(MemoryPayRunAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedPayRunAgreementStore(inner, PayRunAgreementCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PayRunAgreement> FindPayRunAgreements(IPayRunAgreementStore store, PayRunAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
