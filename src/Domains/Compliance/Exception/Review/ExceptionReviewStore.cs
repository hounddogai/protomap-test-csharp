namespace Acme.Workforce.Compliance.Exception.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps exception reviews by code.</summary>
public interface IExceptionReviewStore
{
    ExceptionReview? Find(string code);
    void Save(ExceptionReview item);
    bool Remove(string code);
    List<ExceptionReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryExceptionReviewStore(Dictionary<string, ExceptionReview> items, List<string> order) : IExceptionReviewStore
{
    public Dictionary<string, ExceptionReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ExceptionReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ExceptionReview item)
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

    public List<ExceptionReview> List()
    {
        var outValue = new List<ExceptionReview>();
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
public sealed class CachedExceptionReviewStore(MemoryExceptionReviewStore inner, ExceptionReviewCache cache, int hits, int misses) : IExceptionReviewStore
{
    public MemoryExceptionReviewStore Inner { get; set; } = inner;
    public ExceptionReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ExceptionReview? Find(string code)
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

    public void Save(ExceptionReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ExceptionReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects exception reviews. Empty fields match all.</summary>
public sealed record ExceptionReviewQuery(string Locale, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(ExceptionReview item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ExceptionReview> Apply(List<ExceptionReview> items)
    {
        var matched = Collections.Filter(items, (ExceptionReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of exception review store.</summary>
public static class ExceptionReviewStore
{
    public static IExceptionReviewStore NewMemoryExceptionReviewStore()
    {
        return new MemoryExceptionReviewStore(new Dictionary<string, ExceptionReview>(), new List<string>());
    }

    public static IExceptionReviewStore NewCachedExceptionReviewStore(MemoryExceptionReviewStore inner, int ttl, IClock clock)
    {
        return new CachedExceptionReviewStore(inner, ExceptionReviewCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<ExceptionReview> FindExceptionReviews(IExceptionReviewStore store, ExceptionReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
