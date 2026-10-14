namespace Acme.Workforce.Engagement.Sentiment.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps sentiment groups by code.</summary>
public interface ISentimentGroupStore
{
    SentimentGroup? Find(string code);
    void Save(SentimentGroup item);
    bool Remove(string code);
    List<SentimentGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySentimentGroupStore(Dictionary<string, SentimentGroup> items, List<string> order) : ISentimentGroupStore
{
    public Dictionary<string, SentimentGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SentimentGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SentimentGroup item)
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

    public List<SentimentGroup> List()
    {
        var outValue = new List<SentimentGroup>();
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
public sealed class CachedSentimentGroupStore(MemorySentimentGroupStore inner, SentimentGroupCache cache, int hits, int misses) : ISentimentGroupStore
{
    public MemorySentimentGroupStore Inner { get; set; } = inner;
    public SentimentGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SentimentGroup? Find(string code)
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

    public void Save(SentimentGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SentimentGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects sentiment groups. Empty fields match all.</summary>
public sealed record SentimentGroupQuery(string Segment, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(SentimentGroup item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SentimentGroup> Apply(List<SentimentGroup> items)
    {
        var matched = Collections.Filter(items, (SentimentGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of sentiment group store.</summary>
public static class SentimentGroupStore
{
    public static ISentimentGroupStore NewMemorySentimentGroupStore()
    {
        return new MemorySentimentGroupStore(new Dictionary<string, SentimentGroup>(), new List<string>());
    }

    public static ISentimentGroupStore NewCachedSentimentGroupStore(MemorySentimentGroupStore inner, int ttl, IClock clock)
    {
        return new CachedSentimentGroupStore(inner, SentimentGroupCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<SentimentGroup> FindSentimentGroups(ISentimentGroupStore store, SentimentGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
