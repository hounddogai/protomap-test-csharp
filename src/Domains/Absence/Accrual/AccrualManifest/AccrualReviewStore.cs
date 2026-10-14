namespace Acme.Workforce.Absence.Accrual.AccrualManifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps accrual reviews by code.</summary>
public interface IAccrualReviewStore
{
    AccrualReview? Find(string code);
    void Save(AccrualReview item);
    bool Remove(string code);
    List<AccrualReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccrualReviewStore(Dictionary<string, AccrualReview> items, List<string> order) : IAccrualReviewStore
{
    public Dictionary<string, AccrualReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccrualReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccrualReview item)
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

    public List<AccrualReview> List()
    {
        var outValue = new List<AccrualReview>();
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
public sealed class CachedAccrualReviewStore(MemoryAccrualReviewStore inner, AccrualReviewCache cache, int hits, int misses) : IAccrualReviewStore
{
    public MemoryAccrualReviewStore Inner { get; set; } = inner;
    public AccrualReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccrualReview? Find(string code)
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

    public void Save(AccrualReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccrualReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects accrual reviews. Empty fields match all.</summary>
public sealed record AccrualReviewQuery(string Source, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(AccrualReview item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AccrualReview> Apply(List<AccrualReview> items)
    {
        var matched = Collections.Filter(items, (AccrualReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of accrual review store.</summary>
public static class AccrualReviewStore
{
    public static IAccrualReviewStore NewMemoryAccrualReviewStore()
    {
        return new MemoryAccrualReviewStore(new Dictionary<string, AccrualReview>(), new List<string>());
    }

    public static IAccrualReviewStore NewCachedAccrualReviewStore(MemoryAccrualReviewStore inner, int ttl, IClock clock)
    {
        return new CachedAccrualReviewStore(inner, AccrualReviewCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<AccrualReview> FindAccrualReviews(IAccrualReviewStore store, AccrualReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
