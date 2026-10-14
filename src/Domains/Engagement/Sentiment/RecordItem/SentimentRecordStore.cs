namespace Acme.Workforce.Engagement.Sentiment.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps sentiment records by code.</summary>
public interface ISentimentRecordStore
{
    SentimentRecord? Find(string code);
    void Save(SentimentRecord item);
    bool Remove(string code);
    List<SentimentRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySentimentRecordStore(Dictionary<string, SentimentRecord> items, List<string> order) : ISentimentRecordStore
{
    public Dictionary<string, SentimentRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SentimentRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SentimentRecord item)
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

    public List<SentimentRecord> List()
    {
        var outValue = new List<SentimentRecord>();
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
public sealed class CachedSentimentRecordStore(MemorySentimentRecordStore inner, SentimentRecordCache cache, int hits, int misses) : ISentimentRecordStore
{
    public MemorySentimentRecordStore Inner { get; set; } = inner;
    public SentimentRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SentimentRecord? Find(string code)
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

    public void Save(SentimentRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SentimentRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects sentiment records. Empty fields match all.</summary>
public sealed record SentimentRecordQuery(string Locale, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(SentimentRecord item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SentimentRecord> Apply(List<SentimentRecord> items)
    {
        var matched = Collections.Filter(items, (SentimentRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of sentiment record store.</summary>
public static class SentimentRecordStore
{
    public static ISentimentRecordStore NewMemorySentimentRecordStore()
    {
        return new MemorySentimentRecordStore(new Dictionary<string, SentimentRecord>(), new List<string>());
    }

    public static ISentimentRecordStore NewCachedSentimentRecordStore(MemorySentimentRecordStore inner, int ttl, IClock clock)
    {
        return new CachedSentimentRecordStore(inner, SentimentRecordCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<SentimentRecord> FindSentimentRecords(ISentimentRecordStore store, SentimentRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
