namespace Acme.Workforce.Orgdesign.Span.SpanReview;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional span reviews by code.</summary>
public interface IRegionalSpanReviewStore
{
    RegionalSpanReview? Find(string code);
    void Save(RegionalSpanReview item);
    bool Remove(string code);
    List<RegionalSpanReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalSpanReviewStore(Dictionary<string, RegionalSpanReview> items, List<string> order) : IRegionalSpanReviewStore
{
    public Dictionary<string, RegionalSpanReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalSpanReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalSpanReview item)
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

    public List<RegionalSpanReview> List()
    {
        var outValue = new List<RegionalSpanReview>();
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
public sealed class CachedRegionalSpanReviewStore(MemoryRegionalSpanReviewStore inner, RegionalSpanReviewCache cache, int hits, int misses) : IRegionalSpanReviewStore
{
    public MemoryRegionalSpanReviewStore Inner { get; set; } = inner;
    public RegionalSpanReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalSpanReview? Find(string code)
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

    public void Save(RegionalSpanReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalSpanReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional span reviews. Empty fields match all.</summary>
public sealed record RegionalSpanReviewQuery(string Label, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalSpanReview item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalSpanReview> Apply(List<RegionalSpanReview> items)
    {
        var matched = Collections.Filter(items, (RegionalSpanReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional span review store.</summary>
public static class RegionalSpanReviewStore
{
    public static IRegionalSpanReviewStore NewMemoryRegionalSpanReviewStore()
    {
        return new MemoryRegionalSpanReviewStore(new Dictionary<string, RegionalSpanReview>(), new List<string>());
    }

    public static IRegionalSpanReviewStore NewCachedRegionalSpanReviewStore(MemoryRegionalSpanReviewStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalSpanReviewStore(inner, RegionalSpanReviewCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<RegionalSpanReview> FindRegionalSpanReviews(IRegionalSpanReviewStore store, RegionalSpanReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
