namespace Acme.Workforce.Workplace.AccessBadge.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional access badge reviews by code.</summary>
public interface IRegionalAccessBadgeReviewStore
{
    RegionalAccessBadgeReview? Find(string code);
    void Save(RegionalAccessBadgeReview item);
    bool Remove(string code);
    List<RegionalAccessBadgeReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalAccessBadgeReviewStore(Dictionary<string, RegionalAccessBadgeReview> items, List<string> order) : IRegionalAccessBadgeReviewStore
{
    public Dictionary<string, RegionalAccessBadgeReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalAccessBadgeReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalAccessBadgeReview item)
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

    public List<RegionalAccessBadgeReview> List()
    {
        var outValue = new List<RegionalAccessBadgeReview>();
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
public sealed class CachedRegionalAccessBadgeReviewStore(MemoryRegionalAccessBadgeReviewStore inner, RegionalAccessBadgeReviewCache cache, int hits, int misses) : IRegionalAccessBadgeReviewStore
{
    public MemoryRegionalAccessBadgeReviewStore Inner { get; set; } = inner;
    public RegionalAccessBadgeReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalAccessBadgeReview? Find(string code)
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

    public void Save(RegionalAccessBadgeReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalAccessBadgeReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional access badge reviews. Empty fields match all.</summary>
public sealed record RegionalAccessBadgeReviewQuery(string Department, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalAccessBadgeReview item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalAccessBadgeReview> Apply(List<RegionalAccessBadgeReview> items)
    {
        var matched = Collections.Filter(items, (RegionalAccessBadgeReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional access badge review store.</summary>
public static class RegionalAccessBadgeReviewStore
{
    public static IRegionalAccessBadgeReviewStore NewMemoryRegionalAccessBadgeReviewStore()
    {
        return new MemoryRegionalAccessBadgeReviewStore(
            new Dictionary<string, RegionalAccessBadgeReview>(),
            new List<string>());
    }

    public static IRegionalAccessBadgeReviewStore NewCachedRegionalAccessBadgeReviewStore(MemoryRegionalAccessBadgeReviewStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalAccessBadgeReviewStore(
            inner,
            RegionalAccessBadgeReviewCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<RegionalAccessBadgeReview> FindRegionalAccessBadgeReviews(IRegionalAccessBadgeReviewStore store, RegionalAccessBadgeReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
