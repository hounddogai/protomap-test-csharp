namespace Acme.Workforce.Payrollops.Payslip.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps payslip summaries by code.</summary>
public interface IPayslipSummaryStore
{
    PayslipSummary? Find(string code);
    void Save(PayslipSummary item);
    bool Remove(string code);
    List<PayslipSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayslipSummaryStore(Dictionary<string, PayslipSummary> items, List<string> order) : IPayslipSummaryStore
{
    public Dictionary<string, PayslipSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayslipSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayslipSummary item)
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

    public List<PayslipSummary> List()
    {
        var outValue = new List<PayslipSummary>();
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
public sealed class CachedPayslipSummaryStore(MemoryPayslipSummaryStore inner, PayslipSummaryCache cache, int hits, int misses) : IPayslipSummaryStore
{
    public MemoryPayslipSummaryStore Inner { get; set; } = inner;
    public PayslipSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayslipSummary? Find(string code)
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

    public void Save(PayslipSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayslipSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects payslip summaries. Empty fields match all.</summary>
public sealed record PayslipSummaryQuery(string Channel, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(PayslipSummary item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
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

    public List<PayslipSummary> Apply(List<PayslipSummary> items)
    {
        var matched = Collections.Filter(items, (PayslipSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of payslip summary store.</summary>
public static class PayslipSummaryStore
{
    public static IPayslipSummaryStore NewMemoryPayslipSummaryStore()
    {
        return new MemoryPayslipSummaryStore(new Dictionary<string, PayslipSummary>(), new List<string>());
    }

    public static IPayslipSummaryStore NewCachedPayslipSummaryStore(MemoryPayslipSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedPayslipSummaryStore(inner, PayslipSummaryCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<PayslipSummary> FindPayslipSummaries(IPayslipSummaryStore store, PayslipSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
