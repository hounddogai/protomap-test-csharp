namespace Acme.Workforce.Engagement.Feedback.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary feedback notices by code.</summary>
public interface IPrimaryFeedbackNoticeStore
{
    PrimaryFeedbackNotice? Find(string code);
    void Save(PrimaryFeedbackNotice item);
    bool Remove(string code);
    List<PrimaryFeedbackNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryFeedbackNoticeStore(Dictionary<string, PrimaryFeedbackNotice> items, List<string> order) : IPrimaryFeedbackNoticeStore
{
    public Dictionary<string, PrimaryFeedbackNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryFeedbackNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryFeedbackNotice item)
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

    public List<PrimaryFeedbackNotice> List()
    {
        var outValue = new List<PrimaryFeedbackNotice>();
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
public sealed class CachedPrimaryFeedbackNoticeStore(MemoryPrimaryFeedbackNoticeStore inner, PrimaryFeedbackNoticeCache cache, int hits, int misses) : IPrimaryFeedbackNoticeStore
{
    public MemoryPrimaryFeedbackNoticeStore Inner { get; set; } = inner;
    public PrimaryFeedbackNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryFeedbackNotice? Find(string code)
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

    public void Save(PrimaryFeedbackNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryFeedbackNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary feedback notices. Empty fields match all.</summary>
public sealed record PrimaryFeedbackNoticeQuery(string Title, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryFeedbackNotice item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<PrimaryFeedbackNotice> Apply(List<PrimaryFeedbackNotice> items)
    {
        var matched = Collections.Filter(items, (PrimaryFeedbackNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary feedback notice store.</summary>
public static class PrimaryFeedbackNoticeStore
{
    public static IPrimaryFeedbackNoticeStore NewMemoryPrimaryFeedbackNoticeStore()
    {
        return new MemoryPrimaryFeedbackNoticeStore(
            new Dictionary<string, PrimaryFeedbackNotice>(),
            new List<string>());
    }

    public static IPrimaryFeedbackNoticeStore NewCachedPrimaryFeedbackNoticeStore(MemoryPrimaryFeedbackNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryFeedbackNoticeStore(inner, PrimaryFeedbackNoticeCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PrimaryFeedbackNotice> FindPrimaryFeedbackNotices(IPrimaryFeedbackNoticeStore store, PrimaryFeedbackNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
