namespace Acme.Workforce.Engagement.Sentiment.Manifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled sentiment targets by code.</summary>
public interface IScheduledSentimentTargetStore
{
    ScheduledSentimentTarget? Find(string code);
    void Save(ScheduledSentimentTarget item);
    bool Remove(string code);
    List<ScheduledSentimentTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledSentimentTargetStore(Dictionary<string, ScheduledSentimentTarget> items, List<string> order) : IScheduledSentimentTargetStore
{
    public Dictionary<string, ScheduledSentimentTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledSentimentTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledSentimentTarget item)
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

    public List<ScheduledSentimentTarget> List()
    {
        var outValue = new List<ScheduledSentimentTarget>();
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
public sealed class CachedScheduledSentimentTargetStore(MemoryScheduledSentimentTargetStore inner, ScheduledSentimentTargetCache cache, int hits, int misses) : IScheduledSentimentTargetStore
{
    public MemoryScheduledSentimentTargetStore Inner { get; set; } = inner;
    public ScheduledSentimentTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledSentimentTarget? Find(string code)
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

    public void Save(ScheduledSentimentTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledSentimentTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled sentiment targets. Empty fields match all.</summary>
public sealed record ScheduledSentimentTargetQuery(string Source, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledSentimentTarget item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledSentimentTarget> Apply(List<ScheduledSentimentTarget> items)
    {
        var matched = Collections.Filter(items, (ScheduledSentimentTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled sentiment target store.</summary>
public static class ScheduledSentimentTargetStore
{
    public static IScheduledSentimentTargetStore NewMemoryScheduledSentimentTargetStore()
    {
        return new MemoryScheduledSentimentTargetStore(
            new Dictionary<string, ScheduledSentimentTarget>(),
            new List<string>());
    }

    public static IScheduledSentimentTargetStore NewCachedScheduledSentimentTargetStore(MemoryScheduledSentimentTargetStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledSentimentTargetStore(
            inner,
            ScheduledSentimentTargetCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledSentimentTarget> FindScheduledSentimentTargets(IScheduledSentimentTargetStore store, ScheduledSentimentTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
