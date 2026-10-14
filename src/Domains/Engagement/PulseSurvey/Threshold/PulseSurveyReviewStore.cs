namespace Acme.Workforce.Engagement.PulseSurvey.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pulse survey reviews by code.</summary>
public interface IPulseSurveyReviewStore
{
    PulseSurveyReview? Find(string code);
    void Save(PulseSurveyReview item);
    bool Remove(string code);
    List<PulseSurveyReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPulseSurveyReviewStore(Dictionary<string, PulseSurveyReview> items, List<string> order) : IPulseSurveyReviewStore
{
    public Dictionary<string, PulseSurveyReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PulseSurveyReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PulseSurveyReview item)
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

    public List<PulseSurveyReview> List()
    {
        var outValue = new List<PulseSurveyReview>();
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
public sealed class CachedPulseSurveyReviewStore(MemoryPulseSurveyReviewStore inner, PulseSurveyReviewCache cache, int hits, int misses) : IPulseSurveyReviewStore
{
    public MemoryPulseSurveyReviewStore Inner { get; set; } = inner;
    public PulseSurveyReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PulseSurveyReview? Find(string code)
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

    public void Save(PulseSurveyReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PulseSurveyReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pulse survey reviews. Empty fields match all.</summary>
public sealed record PulseSurveyReviewQuery(string Locale, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(PulseSurveyReview item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PulseSurveyReview> Apply(List<PulseSurveyReview> items)
    {
        var matched = Collections.Filter(items, (PulseSurveyReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pulse survey review store.</summary>
public static class PulseSurveyReviewStore
{
    public static IPulseSurveyReviewStore NewMemoryPulseSurveyReviewStore()
    {
        return new MemoryPulseSurveyReviewStore(new Dictionary<string, PulseSurveyReview>(), new List<string>());
    }

    public static IPulseSurveyReviewStore NewCachedPulseSurveyReviewStore(MemoryPulseSurveyReviewStore inner, int ttl, IClock clock)
    {
        return new CachedPulseSurveyReviewStore(inner, PulseSurveyReviewCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PulseSurveyReview> FindPulseSurveyReviews(IPulseSurveyReviewStore store, PulseSurveyReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
