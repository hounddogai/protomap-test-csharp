namespace Acme.Workforce.Payrollops.Payslip.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps payslip notices by code.</summary>
public interface IPayslipNoticeStore
{
    PayslipNotice? Find(string code);
    void Save(PayslipNotice item);
    bool Remove(string code);
    List<PayslipNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayslipNoticeStore(Dictionary<string, PayslipNotice> items, List<string> order) : IPayslipNoticeStore
{
    public Dictionary<string, PayslipNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayslipNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayslipNotice item)
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

    public List<PayslipNotice> List()
    {
        var outValue = new List<PayslipNotice>();
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
public sealed class CachedPayslipNoticeStore(MemoryPayslipNoticeStore inner, PayslipNoticeCache cache, int hits, int misses) : IPayslipNoticeStore
{
    public MemoryPayslipNoticeStore Inner { get; set; } = inner;
    public PayslipNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayslipNotice? Find(string code)
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

    public void Save(PayslipNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayslipNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects payslip notices. Empty fields match all.</summary>
public sealed record PayslipNoticeQuery(string Label, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(PayslipNotice item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<PayslipNotice> Apply(List<PayslipNotice> items)
    {
        var matched = Collections.Filter(items, (PayslipNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of payslip notice store.</summary>
public static class PayslipNoticeStore
{
    public static IPayslipNoticeStore NewMemoryPayslipNoticeStore()
    {
        return new MemoryPayslipNoticeStore(new Dictionary<string, PayslipNotice>(), new List<string>());
    }

    public static IPayslipNoticeStore NewCachedPayslipNoticeStore(MemoryPayslipNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedPayslipNoticeStore(inner, PayslipNoticeCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PayslipNotice> FindPayslipNotices(IPayslipNoticeStore store, PayslipNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
