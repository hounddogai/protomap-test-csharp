namespace Acme.Workforce.Engagement.Feedback.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps feedback mappings by code.</summary>
public interface IFeedbackMappingStore
{
    FeedbackMapping? Find(string code);
    void Save(FeedbackMapping item);
    bool Remove(string code);
    List<FeedbackMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFeedbackMappingStore(Dictionary<string, FeedbackMapping> items, List<string> order) : IFeedbackMappingStore
{
    public Dictionary<string, FeedbackMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FeedbackMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FeedbackMapping item)
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

    public List<FeedbackMapping> List()
    {
        var outValue = new List<FeedbackMapping>();
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
public sealed class CachedFeedbackMappingStore(MemoryFeedbackMappingStore inner, FeedbackMappingCache cache, int hits, int misses) : IFeedbackMappingStore
{
    public MemoryFeedbackMappingStore Inner { get; set; } = inner;
    public FeedbackMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FeedbackMapping? Find(string code)
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

    public void Save(FeedbackMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FeedbackMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects feedback mappings. Empty fields match all.</summary>
public sealed record FeedbackMappingQuery(string Title, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(FeedbackMapping item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FeedbackMapping> Apply(List<FeedbackMapping> items)
    {
        var matched = Collections.Filter(items, (FeedbackMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of feedback mapping store.</summary>
public static class FeedbackMappingStore
{
    public static IFeedbackMappingStore NewMemoryFeedbackMappingStore()
    {
        return new MemoryFeedbackMappingStore(new Dictionary<string, FeedbackMapping>(), new List<string>());
    }

    public static IFeedbackMappingStore NewCachedFeedbackMappingStore(MemoryFeedbackMappingStore inner, int ttl, IClock clock)
    {
        return new CachedFeedbackMappingStore(inner, FeedbackMappingCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<FeedbackMapping> FindFeedbackMappings(IFeedbackMappingStore store, FeedbackMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
