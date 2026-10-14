namespace Acme.Workforce.Payrollops.Payslip.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps payslip drafts by code.</summary>
public interface IPayslipDraftStore
{
    PayslipDraft? Find(string code);
    void Save(PayslipDraft item);
    bool Remove(string code);
    List<PayslipDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayslipDraftStore(Dictionary<string, PayslipDraft> items, List<string> order) : IPayslipDraftStore
{
    public Dictionary<string, PayslipDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayslipDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayslipDraft item)
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

    public List<PayslipDraft> List()
    {
        var outValue = new List<PayslipDraft>();
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
public sealed class CachedPayslipDraftStore(MemoryPayslipDraftStore inner, PayslipDraftCache cache, int hits, int misses) : IPayslipDraftStore
{
    public MemoryPayslipDraftStore Inner { get; set; } = inner;
    public PayslipDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayslipDraft? Find(string code)
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

    public void Save(PayslipDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayslipDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects payslip drafts. Empty fields match all.</summary>
public sealed record PayslipDraftQuery(string Name, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(PayslipDraft item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<PayslipDraft> Apply(List<PayslipDraft> items)
    {
        var matched = Collections.Filter(items, (PayslipDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of payslip draft store.</summary>
public static class PayslipDraftStore
{
    public static IPayslipDraftStore NewMemoryPayslipDraftStore()
    {
        return new MemoryPayslipDraftStore(new Dictionary<string, PayslipDraft>(), new List<string>());
    }

    public static IPayslipDraftStore NewCachedPayslipDraftStore(MemoryPayslipDraftStore inner, int ttl, IClock clock)
    {
        return new CachedPayslipDraftStore(inner, PayslipDraftCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PayslipDraft> FindPayslipDrafts(IPayslipDraftStore store, PayslipDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
