namespace Acme.Workforce.Engagement.Recognition.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps recognition reviews by code.</summary>
public interface IRecognitionReviewStore
{
    RecognitionReview? Find(string code);
    void Save(RecognitionReview item);
    bool Remove(string code);
    List<RecognitionReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRecognitionReviewStore(Dictionary<string, RecognitionReview> items, List<string> order) : IRecognitionReviewStore
{
    public Dictionary<string, RecognitionReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RecognitionReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RecognitionReview item)
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

    public List<RecognitionReview> List()
    {
        var outValue = new List<RecognitionReview>();
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
public sealed class CachedRecognitionReviewStore(MemoryRecognitionReviewStore inner, RecognitionReviewCache cache, int hits, int misses) : IRecognitionReviewStore
{
    public MemoryRecognitionReviewStore Inner { get; set; } = inner;
    public RecognitionReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RecognitionReview? Find(string code)
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

    public void Save(RecognitionReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RecognitionReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects recognition reviews. Empty fields match all.</summary>
public sealed record RecognitionReviewQuery(string Region, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(RecognitionReview item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RecognitionReview> Apply(List<RecognitionReview> items)
    {
        var matched = Collections.Filter(items, (RecognitionReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of recognition review store.</summary>
public static class RecognitionReviewStore
{
    public static IRecognitionReviewStore NewMemoryRecognitionReviewStore()
    {
        return new MemoryRecognitionReviewStore(new Dictionary<string, RecognitionReview>(), new List<string>());
    }

    public static IRecognitionReviewStore NewCachedRecognitionReviewStore(MemoryRecognitionReviewStore inner, int ttl, IClock clock)
    {
        return new CachedRecognitionReviewStore(inner, RecognitionReviewCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<RecognitionReview> FindRecognitionReviews(IRecognitionReviewStore store, RecognitionReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
