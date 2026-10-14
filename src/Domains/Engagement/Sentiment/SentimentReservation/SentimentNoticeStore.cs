namespace Acme.Workforce.Engagement.Sentiment.SentimentReservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps sentiment notices by code.</summary>
public interface ISentimentNoticeStore
{
    SentimentNotice? Find(string code);
    void Save(SentimentNotice item);
    bool Remove(string code);
    List<SentimentNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySentimentNoticeStore(Dictionary<string, SentimentNotice> items, List<string> order) : ISentimentNoticeStore
{
    public Dictionary<string, SentimentNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SentimentNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SentimentNotice item)
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

    public List<SentimentNotice> List()
    {
        var outValue = new List<SentimentNotice>();
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
public sealed class CachedSentimentNoticeStore(MemorySentimentNoticeStore inner, SentimentNoticeCache cache, int hits, int misses) : ISentimentNoticeStore
{
    public MemorySentimentNoticeStore Inner { get; set; } = inner;
    public SentimentNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SentimentNotice? Find(string code)
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

    public void Save(SentimentNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SentimentNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects sentiment notices. Empty fields match all.</summary>
public sealed record SentimentNoticeQuery(string Channel, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(SentimentNotice item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SentimentNotice> Apply(List<SentimentNotice> items)
    {
        var matched = Collections.Filter(items, (SentimentNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of sentiment notice store.</summary>
public static class SentimentNoticeStore
{
    public static ISentimentNoticeStore NewMemorySentimentNoticeStore()
    {
        return new MemorySentimentNoticeStore(new Dictionary<string, SentimentNotice>(), new List<string>());
    }

    public static ISentimentNoticeStore NewCachedSentimentNoticeStore(MemorySentimentNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedSentimentNoticeStore(inner, SentimentNoticeCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<SentimentNotice> FindSentimentNotices(ISentimentNoticeStore store, SentimentNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
