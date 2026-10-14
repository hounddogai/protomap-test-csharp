namespace Acme.Workforce.Engagement.Recognition.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled recognition thresholds by code.</summary>
public interface IScheduledRecognitionThresholdStore
{
    ScheduledRecognitionThreshold? Find(string code);
    void Save(ScheduledRecognitionThreshold item);
    bool Remove(string code);
    List<ScheduledRecognitionThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledRecognitionThresholdStore(Dictionary<string, ScheduledRecognitionThreshold> items, List<string> order) : IScheduledRecognitionThresholdStore
{
    public Dictionary<string, ScheduledRecognitionThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledRecognitionThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledRecognitionThreshold item)
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

    public List<ScheduledRecognitionThreshold> List()
    {
        var outValue = new List<ScheduledRecognitionThreshold>();
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
public sealed class CachedScheduledRecognitionThresholdStore(MemoryScheduledRecognitionThresholdStore inner, ScheduledRecognitionThresholdCache cache, int hits, int misses) : IScheduledRecognitionThresholdStore
{
    public MemoryScheduledRecognitionThresholdStore Inner { get; set; } = inner;
    public ScheduledRecognitionThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledRecognitionThreshold? Find(string code)
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

    public void Save(ScheduledRecognitionThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledRecognitionThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled recognition thresholds. Empty fields match all.</summary>
public sealed record ScheduledRecognitionThresholdQuery(string Region, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledRecognitionThreshold item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledRecognitionThreshold> Apply(List<ScheduledRecognitionThreshold> items)
    {
        var matched = Collections.Filter(items, (ScheduledRecognitionThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled recognition threshold store.</summary>
public static class ScheduledRecognitionThresholdStore
{
    public static IScheduledRecognitionThresholdStore NewMemoryScheduledRecognitionThresholdStore()
    {
        return new MemoryScheduledRecognitionThresholdStore(
            new Dictionary<string, ScheduledRecognitionThreshold>(),
            new List<string>());
    }

    public static IScheduledRecognitionThresholdStore NewCachedScheduledRecognitionThresholdStore(MemoryScheduledRecognitionThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledRecognitionThresholdStore(
            inner,
            ScheduledRecognitionThresholdCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledRecognitionThreshold> FindScheduledRecognitionThresholds(IScheduledRecognitionThresholdStore store, ScheduledRecognitionThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
