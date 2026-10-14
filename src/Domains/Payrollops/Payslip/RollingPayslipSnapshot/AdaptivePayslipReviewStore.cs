namespace Acme.Workforce.Payrollops.Payslip.RollingPayslipSnapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive payslip reviews by code.</summary>
public interface IAdaptivePayslipReviewStore
{
    AdaptivePayslipReview? Find(string code);
    void Save(AdaptivePayslipReview item);
    bool Remove(string code);
    List<AdaptivePayslipReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptivePayslipReviewStore(Dictionary<string, AdaptivePayslipReview> items, List<string> order) : IAdaptivePayslipReviewStore
{
    public Dictionary<string, AdaptivePayslipReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptivePayslipReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptivePayslipReview item)
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

    public List<AdaptivePayslipReview> List()
    {
        var outValue = new List<AdaptivePayslipReview>();
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
public sealed class CachedAdaptivePayslipReviewStore(MemoryAdaptivePayslipReviewStore inner, AdaptivePayslipReviewCache cache, int hits, int misses) : IAdaptivePayslipReviewStore
{
    public MemoryAdaptivePayslipReviewStore Inner { get; set; } = inner;
    public AdaptivePayslipReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptivePayslipReview? Find(string code)
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

    public void Save(AdaptivePayslipReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptivePayslipReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive payslip reviews. Empty fields match all.</summary>
public sealed record AdaptivePayslipReviewQuery(string Name, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptivePayslipReview item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptivePayslipReview> Apply(List<AdaptivePayslipReview> items)
    {
        var matched = Collections.Filter(items, (AdaptivePayslipReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive payslip review store.</summary>
public static class AdaptivePayslipReviewStore
{
    public static IAdaptivePayslipReviewStore NewMemoryAdaptivePayslipReviewStore()
    {
        return new MemoryAdaptivePayslipReviewStore(
            new Dictionary<string, AdaptivePayslipReview>(),
            new List<string>());
    }

    public static IAdaptivePayslipReviewStore NewCachedAdaptivePayslipReviewStore(MemoryAdaptivePayslipReviewStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptivePayslipReviewStore(inner, AdaptivePayslipReviewCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<AdaptivePayslipReview> FindAdaptivePayslipReviews(IAdaptivePayslipReviewStore store, AdaptivePayslipReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
