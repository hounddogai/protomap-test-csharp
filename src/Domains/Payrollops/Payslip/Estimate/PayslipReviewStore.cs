namespace Acme.Workforce.Payrollops.Payslip.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps payslip reviews by code.</summary>
public interface IPayslipReviewStore
{
    PayslipReview? Find(string code);
    void Save(PayslipReview item);
    bool Remove(string code);
    List<PayslipReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayslipReviewStore(Dictionary<string, PayslipReview> items, List<string> order) : IPayslipReviewStore
{
    public Dictionary<string, PayslipReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayslipReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayslipReview item)
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

    public List<PayslipReview> List()
    {
        var outValue = new List<PayslipReview>();
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
public sealed class CachedPayslipReviewStore(MemoryPayslipReviewStore inner, PayslipReviewCache cache, int hits, int misses) : IPayslipReviewStore
{
    public MemoryPayslipReviewStore Inner { get; set; } = inner;
    public PayslipReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayslipReview? Find(string code)
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

    public void Save(PayslipReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayslipReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects payslip reviews. Empty fields match all.</summary>
public sealed record PayslipReviewQuery(string Title, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(PayslipReview item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayslipReview> Apply(List<PayslipReview> items)
    {
        var matched = Collections.Filter(items, (PayslipReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of payslip review store.</summary>
public static class PayslipReviewStore
{
    public static IPayslipReviewStore NewMemoryPayslipReviewStore()
    {
        return new MemoryPayslipReviewStore(new Dictionary<string, PayslipReview>(), new List<string>());
    }

    public static IPayslipReviewStore NewCachedPayslipReviewStore(MemoryPayslipReviewStore inner, int ttl, IClock clock)
    {
        return new CachedPayslipReviewStore(inner, PayslipReviewCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PayslipReview> FindPayslipReviews(IPayslipReviewStore store, PayslipReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
