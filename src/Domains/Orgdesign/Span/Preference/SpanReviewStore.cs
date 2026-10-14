namespace Acme.Workforce.Orgdesign.Span.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps span reviews by code.</summary>
public interface ISpanReviewStore
{
    SpanReview? Find(string code);
    void Save(SpanReview item);
    bool Remove(string code);
    List<SpanReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySpanReviewStore(Dictionary<string, SpanReview> items, List<string> order) : ISpanReviewStore
{
    public Dictionary<string, SpanReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SpanReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SpanReview item)
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

    public List<SpanReview> List()
    {
        var outValue = new List<SpanReview>();
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
public sealed class CachedSpanReviewStore(MemorySpanReviewStore inner, SpanReviewCache cache, int hits, int misses) : ISpanReviewStore
{
    public MemorySpanReviewStore Inner { get; set; } = inner;
    public SpanReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SpanReview? Find(string code)
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

    public void Save(SpanReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SpanReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects span reviews. Empty fields match all.</summary>
public sealed record SpanReviewQuery(string Region, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(SpanReview item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SpanReview> Apply(List<SpanReview> items)
    {
        var matched = Collections.Filter(items, (SpanReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of span review store.</summary>
public static class SpanReviewStore
{
    public static ISpanReviewStore NewMemorySpanReviewStore()
    {
        return new MemorySpanReviewStore(new Dictionary<string, SpanReview>(), new List<string>());
    }

    public static ISpanReviewStore NewCachedSpanReviewStore(MemorySpanReviewStore inner, int ttl, IClock clock)
    {
        return new CachedSpanReviewStore(inner, SpanReviewCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<SpanReview> FindSpanReviews(ISpanReviewStore store, SpanReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
