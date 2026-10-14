namespace Acme.Workforce.Payrollops.Garnishment.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped garnishment reviews by code.</summary>
public interface ICappedGarnishmentReviewStore
{
    CappedGarnishmentReview? Find(string code);
    void Save(CappedGarnishmentReview item);
    bool Remove(string code);
    List<CappedGarnishmentReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedGarnishmentReviewStore(Dictionary<string, CappedGarnishmentReview> items, List<string> order) : ICappedGarnishmentReviewStore
{
    public Dictionary<string, CappedGarnishmentReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedGarnishmentReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedGarnishmentReview item)
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

    public List<CappedGarnishmentReview> List()
    {
        var outValue = new List<CappedGarnishmentReview>();
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
public sealed class CachedCappedGarnishmentReviewStore(MemoryCappedGarnishmentReviewStore inner, CappedGarnishmentReviewCache cache, int hits, int misses) : ICappedGarnishmentReviewStore
{
    public MemoryCappedGarnishmentReviewStore Inner { get; set; } = inner;
    public CappedGarnishmentReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedGarnishmentReview? Find(string code)
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

    public void Save(CappedGarnishmentReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedGarnishmentReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped garnishment reviews. Empty fields match all.</summary>
public sealed record CappedGarnishmentReviewQuery(string Unit, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(CappedGarnishmentReview item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedGarnishmentReview> Apply(List<CappedGarnishmentReview> items)
    {
        var matched = Collections.Filter(items, (CappedGarnishmentReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped garnishment review store.</summary>
public static class CappedGarnishmentReviewStore
{
    public static ICappedGarnishmentReviewStore NewMemoryCappedGarnishmentReviewStore()
    {
        return new MemoryCappedGarnishmentReviewStore(
            new Dictionary<string, CappedGarnishmentReview>(),
            new List<string>());
    }

    public static ICappedGarnishmentReviewStore NewCachedCappedGarnishmentReviewStore(MemoryCappedGarnishmentReviewStore inner, int ttl, IClock clock)
    {
        return new CachedCappedGarnishmentReviewStore(
            inner,
            CappedGarnishmentReviewCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<CappedGarnishmentReview> FindCappedGarnishmentReviews(ICappedGarnishmentReviewStore store, CappedGarnishmentReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
