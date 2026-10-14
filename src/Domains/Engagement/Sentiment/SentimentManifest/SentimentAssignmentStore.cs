namespace Acme.Workforce.Engagement.Sentiment.SentimentManifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps sentiment assignments by code.</summary>
public interface ISentimentAssignmentStore
{
    SentimentAssignment? Find(string code);
    void Save(SentimentAssignment item);
    bool Remove(string code);
    List<SentimentAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySentimentAssignmentStore(Dictionary<string, SentimentAssignment> items, List<string> order) : ISentimentAssignmentStore
{
    public Dictionary<string, SentimentAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SentimentAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SentimentAssignment item)
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

    public List<SentimentAssignment> List()
    {
        var outValue = new List<SentimentAssignment>();
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
public sealed class CachedSentimentAssignmentStore(MemorySentimentAssignmentStore inner, SentimentAssignmentCache cache, int hits, int misses) : ISentimentAssignmentStore
{
    public MemorySentimentAssignmentStore Inner { get; set; } = inner;
    public SentimentAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SentimentAssignment? Find(string code)
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

    public void Save(SentimentAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SentimentAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects sentiment assignments. Empty fields match all.</summary>
public sealed record SentimentAssignmentQuery(string Title, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(SentimentAssignment item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SentimentAssignment> Apply(List<SentimentAssignment> items)
    {
        var matched = Collections.Filter(items, (SentimentAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of sentiment assignment store.</summary>
public static class SentimentAssignmentStore
{
    public static ISentimentAssignmentStore NewMemorySentimentAssignmentStore()
    {
        return new MemorySentimentAssignmentStore(new Dictionary<string, SentimentAssignment>(), new List<string>());
    }

    public static ISentimentAssignmentStore NewCachedSentimentAssignmentStore(MemorySentimentAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedSentimentAssignmentStore(inner, SentimentAssignmentCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<SentimentAssignment> FindSentimentAssignments(ISentimentAssignmentStore store, SentimentAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
