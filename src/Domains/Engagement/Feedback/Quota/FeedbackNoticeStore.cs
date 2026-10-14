namespace Acme.Workforce.Engagement.Feedback.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps feedback notices by code.</summary>
public interface IFeedbackNoticeStore
{
    FeedbackNotice? Find(string code);
    void Save(FeedbackNotice item);
    bool Remove(string code);
    List<FeedbackNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFeedbackNoticeStore(Dictionary<string, FeedbackNotice> items, List<string> order) : IFeedbackNoticeStore
{
    public Dictionary<string, FeedbackNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FeedbackNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FeedbackNotice item)
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

    public List<FeedbackNotice> List()
    {
        var outValue = new List<FeedbackNotice>();
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
public sealed class CachedFeedbackNoticeStore(MemoryFeedbackNoticeStore inner, FeedbackNoticeCache cache, int hits, int misses) : IFeedbackNoticeStore
{
    public MemoryFeedbackNoticeStore Inner { get; set; } = inner;
    public FeedbackNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FeedbackNotice? Find(string code)
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

    public void Save(FeedbackNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FeedbackNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects feedback notices. Empty fields match all.</summary>
public sealed record FeedbackNoticeQuery(string Label, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(FeedbackNotice item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FeedbackNotice> Apply(List<FeedbackNotice> items)
    {
        var matched = Collections.Filter(items, (FeedbackNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of feedback notice store.</summary>
public static class FeedbackNoticeStore
{
    public static IFeedbackNoticeStore NewMemoryFeedbackNoticeStore()
    {
        return new MemoryFeedbackNoticeStore(new Dictionary<string, FeedbackNotice>(), new List<string>());
    }

    public static IFeedbackNoticeStore NewCachedFeedbackNoticeStore(MemoryFeedbackNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedFeedbackNoticeStore(inner, FeedbackNoticeCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<FeedbackNotice> FindFeedbackNotices(IFeedbackNoticeStore store, FeedbackNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
