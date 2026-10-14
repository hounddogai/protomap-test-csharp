namespace Acme.Workforce.Engagement.Sentiment.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps sentiment entries by code.</summary>
public interface ISentimentEntryStore
{
    SentimentEntry? Find(string code);
    void Save(SentimentEntry item);
    bool Remove(string code);
    List<SentimentEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySentimentEntryStore(Dictionary<string, SentimentEntry> items, List<string> order) : ISentimentEntryStore
{
    public Dictionary<string, SentimentEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SentimentEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SentimentEntry item)
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

    public List<SentimentEntry> List()
    {
        var outValue = new List<SentimentEntry>();
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
public sealed class CachedSentimentEntryStore(MemorySentimentEntryStore inner, SentimentEntryCache cache, int hits, int misses) : ISentimentEntryStore
{
    public MemorySentimentEntryStore Inner { get; set; } = inner;
    public SentimentEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SentimentEntry? Find(string code)
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

    public void Save(SentimentEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SentimentEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects sentiment entries. Empty fields match all.</summary>
public sealed record SentimentEntryQuery(string Name, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(SentimentEntry item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<SentimentEntry> Apply(List<SentimentEntry> items)
    {
        var matched = Collections.Filter(items, (SentimentEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of sentiment entry store.</summary>
public static class SentimentEntryStore
{
    public static ISentimentEntryStore NewMemorySentimentEntryStore()
    {
        return new MemorySentimentEntryStore(new Dictionary<string, SentimentEntry>(), new List<string>());
    }

    public static ISentimentEntryStore NewCachedSentimentEntryStore(MemorySentimentEntryStore inner, int ttl, IClock clock)
    {
        return new CachedSentimentEntryStore(inner, SentimentEntryCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<SentimentEntry> FindSentimentEntries(ISentimentEntryStore store, SentimentEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
