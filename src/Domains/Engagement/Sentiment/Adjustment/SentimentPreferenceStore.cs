namespace Acme.Workforce.Engagement.Sentiment.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps sentiment preferences by code.</summary>
public interface ISentimentPreferenceStore
{
    SentimentPreference? Find(string code);
    void Save(SentimentPreference item);
    bool Remove(string code);
    List<SentimentPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySentimentPreferenceStore(Dictionary<string, SentimentPreference> items, List<string> order) : ISentimentPreferenceStore
{
    public Dictionary<string, SentimentPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SentimentPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SentimentPreference item)
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

    public List<SentimentPreference> List()
    {
        var outValue = new List<SentimentPreference>();
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
public sealed class CachedSentimentPreferenceStore(MemorySentimentPreferenceStore inner, SentimentPreferenceCache cache, int hits, int misses) : ISentimentPreferenceStore
{
    public MemorySentimentPreferenceStore Inner { get; set; } = inner;
    public SentimentPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SentimentPreference? Find(string code)
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

    public void Save(SentimentPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SentimentPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects sentiment preferences. Empty fields match all.</summary>
public sealed record SentimentPreferenceQuery(string Channel, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(SentimentPreference item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SentimentPreference> Apply(List<SentimentPreference> items)
    {
        var matched = Collections.Filter(items, (SentimentPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of sentiment preference store.</summary>
public static class SentimentPreferenceStore
{
    public static ISentimentPreferenceStore NewMemorySentimentPreferenceStore()
    {
        return new MemorySentimentPreferenceStore(new Dictionary<string, SentimentPreference>(), new List<string>());
    }

    public static ISentimentPreferenceStore NewCachedSentimentPreferenceStore(MemorySentimentPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedSentimentPreferenceStore(inner, SentimentPreferenceCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<SentimentPreference> FindSentimentPreferences(ISentimentPreferenceStore store, SentimentPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
