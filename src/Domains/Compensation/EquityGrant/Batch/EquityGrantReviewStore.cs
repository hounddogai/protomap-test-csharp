namespace Acme.Workforce.Compensation.EquityGrant.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps equity grant reviews by code.</summary>
public interface IEquityGrantReviewStore
{
    EquityGrantReview? Find(string code);
    void Save(EquityGrantReview item);
    bool Remove(string code);
    List<EquityGrantReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryEquityGrantReviewStore(Dictionary<string, EquityGrantReview> items, List<string> order) : IEquityGrantReviewStore
{
    public Dictionary<string, EquityGrantReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public EquityGrantReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(EquityGrantReview item)
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

    public List<EquityGrantReview> List()
    {
        var outValue = new List<EquityGrantReview>();
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
public sealed class CachedEquityGrantReviewStore(MemoryEquityGrantReviewStore inner, EquityGrantReviewCache cache, int hits, int misses) : IEquityGrantReviewStore
{
    public MemoryEquityGrantReviewStore Inner { get; set; } = inner;
    public EquityGrantReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public EquityGrantReview? Find(string code)
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

    public void Save(EquityGrantReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<EquityGrantReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects equity grant reviews. Empty fields match all.</summary>
public sealed record EquityGrantReviewQuery(string Name, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(EquityGrantReview item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<EquityGrantReview> Apply(List<EquityGrantReview> items)
    {
        var matched = Collections.Filter(items, (EquityGrantReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of equity grant review store.</summary>
public static class EquityGrantReviewStore
{
    public static IEquityGrantReviewStore NewMemoryEquityGrantReviewStore()
    {
        return new MemoryEquityGrantReviewStore(new Dictionary<string, EquityGrantReview>(), new List<string>());
    }

    public static IEquityGrantReviewStore NewCachedEquityGrantReviewStore(MemoryEquityGrantReviewStore inner, int ttl, IClock clock)
    {
        return new CachedEquityGrantReviewStore(inner, EquityGrantReviewCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<EquityGrantReview> FindEquityGrantReviews(IEquityGrantReviewStore store, EquityGrantReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
