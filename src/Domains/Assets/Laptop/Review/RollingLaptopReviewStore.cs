namespace Acme.Workforce.Assets.Laptop.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling laptop reviews by code.</summary>
public interface IRollingLaptopReviewStore
{
    RollingLaptopReview? Find(string code);
    void Save(RollingLaptopReview item);
    bool Remove(string code);
    List<RollingLaptopReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingLaptopReviewStore(Dictionary<string, RollingLaptopReview> items, List<string> order) : IRollingLaptopReviewStore
{
    public Dictionary<string, RollingLaptopReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingLaptopReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingLaptopReview item)
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

    public List<RollingLaptopReview> List()
    {
        var outValue = new List<RollingLaptopReview>();
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
public sealed class CachedRollingLaptopReviewStore(MemoryRollingLaptopReviewStore inner, RollingLaptopReviewCache cache, int hits, int misses) : IRollingLaptopReviewStore
{
    public MemoryRollingLaptopReviewStore Inner { get; set; } = inner;
    public RollingLaptopReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingLaptopReview? Find(string code)
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

    public void Save(RollingLaptopReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingLaptopReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling laptop reviews. Empty fields match all.</summary>
public sealed record RollingLaptopReviewQuery(string Department, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(RollingLaptopReview item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingLaptopReview> Apply(List<RollingLaptopReview> items)
    {
        var matched = Collections.Filter(items, (RollingLaptopReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling laptop review store.</summary>
public static class RollingLaptopReviewStore
{
    public static IRollingLaptopReviewStore NewMemoryRollingLaptopReviewStore()
    {
        return new MemoryRollingLaptopReviewStore(new Dictionary<string, RollingLaptopReview>(), new List<string>());
    }

    public static IRollingLaptopReviewStore NewCachedRollingLaptopReviewStore(MemoryRollingLaptopReviewStore inner, int ttl, IClock clock)
    {
        return new CachedRollingLaptopReviewStore(inner, RollingLaptopReviewCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<RollingLaptopReview> FindRollingLaptopReviews(IRollingLaptopReviewStore store, RollingLaptopReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
