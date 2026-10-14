namespace Acme.Workforce.Payrollops.Deduction.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps deduction reviews by code.</summary>
public interface IDeductionReviewStore
{
    DeductionReview? Find(string code);
    void Save(DeductionReview item);
    bool Remove(string code);
    List<DeductionReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeductionReviewStore(Dictionary<string, DeductionReview> items, List<string> order) : IDeductionReviewStore
{
    public Dictionary<string, DeductionReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeductionReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeductionReview item)
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

    public List<DeductionReview> List()
    {
        var outValue = new List<DeductionReview>();
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
public sealed class CachedDeductionReviewStore(MemoryDeductionReviewStore inner, DeductionReviewCache cache, int hits, int misses) : IDeductionReviewStore
{
    public MemoryDeductionReviewStore Inner { get; set; } = inner;
    public DeductionReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeductionReview? Find(string code)
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

    public void Save(DeductionReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeductionReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects deduction reviews. Empty fields match all.</summary>
public sealed record DeductionReviewQuery(string Department, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(DeductionReview item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DeductionReview> Apply(List<DeductionReview> items)
    {
        var matched = Collections.Filter(items, (DeductionReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of deduction review store.</summary>
public static class DeductionReviewStore
{
    public static IDeductionReviewStore NewMemoryDeductionReviewStore()
    {
        return new MemoryDeductionReviewStore(new Dictionary<string, DeductionReview>(), new List<string>());
    }

    public static IDeductionReviewStore NewCachedDeductionReviewStore(MemoryDeductionReviewStore inner, int ttl, IClock clock)
    {
        return new CachedDeductionReviewStore(inner, DeductionReviewCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<DeductionReview> FindDeductionReviews(IDeductionReviewStore store, DeductionReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
