namespace Acme.Workforce.Engagement.Sentiment.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default sentiment assignments by code.</summary>
public interface IDefaultSentimentAssignmentStore
{
    DefaultSentimentAssignment? Find(string code);
    void Save(DefaultSentimentAssignment item);
    bool Remove(string code);
    List<DefaultSentimentAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultSentimentAssignmentStore(Dictionary<string, DefaultSentimentAssignment> items, List<string> order) : IDefaultSentimentAssignmentStore
{
    public Dictionary<string, DefaultSentimentAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultSentimentAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultSentimentAssignment item)
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

    public List<DefaultSentimentAssignment> List()
    {
        var outValue = new List<DefaultSentimentAssignment>();
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
public sealed class CachedDefaultSentimentAssignmentStore(MemoryDefaultSentimentAssignmentStore inner, DefaultSentimentAssignmentCache cache, int hits, int misses) : IDefaultSentimentAssignmentStore
{
    public MemoryDefaultSentimentAssignmentStore Inner { get; set; } = inner;
    public DefaultSentimentAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultSentimentAssignment? Find(string code)
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

    public void Save(DefaultSentimentAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultSentimentAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default sentiment assignments. Empty fields match all.</summary>
public sealed record DefaultSentimentAssignmentQuery(string Category, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultSentimentAssignment item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultSentimentAssignment> Apply(List<DefaultSentimentAssignment> items)
    {
        var matched = Collections.Filter(items, (DefaultSentimentAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default sentiment assignment store.</summary>
public static class DefaultSentimentAssignmentStore
{
    public static IDefaultSentimentAssignmentStore NewMemoryDefaultSentimentAssignmentStore()
    {
        return new MemoryDefaultSentimentAssignmentStore(
            new Dictionary<string, DefaultSentimentAssignment>(),
            new List<string>());
    }

    public static IDefaultSentimentAssignmentStore NewCachedDefaultSentimentAssignmentStore(MemoryDefaultSentimentAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultSentimentAssignmentStore(
            inner,
            DefaultSentimentAssignmentCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<DefaultSentimentAssignment> FindDefaultSentimentAssignments(IDefaultSentimentAssignmentStore store, DefaultSentimentAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
