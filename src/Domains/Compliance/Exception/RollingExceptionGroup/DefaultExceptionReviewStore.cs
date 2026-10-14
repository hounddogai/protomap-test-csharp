namespace Acme.Workforce.Compliance.Exception.RollingExceptionGroup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default exception reviews by code.</summary>
public interface IDefaultExceptionReviewStore
{
    DefaultExceptionReview? Find(string code);
    void Save(DefaultExceptionReview item);
    bool Remove(string code);
    List<DefaultExceptionReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultExceptionReviewStore(Dictionary<string, DefaultExceptionReview> items, List<string> order) : IDefaultExceptionReviewStore
{
    public Dictionary<string, DefaultExceptionReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultExceptionReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultExceptionReview item)
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

    public List<DefaultExceptionReview> List()
    {
        var outValue = new List<DefaultExceptionReview>();
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
public sealed class CachedDefaultExceptionReviewStore(MemoryDefaultExceptionReviewStore inner, DefaultExceptionReviewCache cache, int hits, int misses) : IDefaultExceptionReviewStore
{
    public MemoryDefaultExceptionReviewStore Inner { get; set; } = inner;
    public DefaultExceptionReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultExceptionReview? Find(string code)
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

    public void Save(DefaultExceptionReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultExceptionReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default exception reviews. Empty fields match all.</summary>
public sealed record DefaultExceptionReviewQuery(string Team, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultExceptionReview item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultExceptionReview> Apply(List<DefaultExceptionReview> items)
    {
        var matched = Collections.Filter(items, (DefaultExceptionReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default exception review store.</summary>
public static class DefaultExceptionReviewStore
{
    public static IDefaultExceptionReviewStore NewMemoryDefaultExceptionReviewStore()
    {
        return new MemoryDefaultExceptionReviewStore(
            new Dictionary<string, DefaultExceptionReview>(),
            new List<string>());
    }

    public static IDefaultExceptionReviewStore NewCachedDefaultExceptionReviewStore(MemoryDefaultExceptionReviewStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultExceptionReviewStore(
            inner,
            DefaultExceptionReviewCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<DefaultExceptionReview> FindDefaultExceptionReviews(IDefaultExceptionReviewStore store, DefaultExceptionReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
