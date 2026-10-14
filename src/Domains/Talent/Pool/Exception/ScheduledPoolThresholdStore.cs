namespace Acme.Workforce.Talent.Pool.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled pool thresholds by code.</summary>
public interface IScheduledPoolThresholdStore
{
    ScheduledPoolThreshold? Find(string code);
    void Save(ScheduledPoolThreshold item);
    bool Remove(string code);
    List<ScheduledPoolThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledPoolThresholdStore(Dictionary<string, ScheduledPoolThreshold> items, List<string> order) : IScheduledPoolThresholdStore
{
    public Dictionary<string, ScheduledPoolThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledPoolThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledPoolThreshold item)
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

    public List<ScheduledPoolThreshold> List()
    {
        var outValue = new List<ScheduledPoolThreshold>();
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
public sealed class CachedScheduledPoolThresholdStore(MemoryScheduledPoolThresholdStore inner, ScheduledPoolThresholdCache cache, int hits, int misses) : IScheduledPoolThresholdStore
{
    public MemoryScheduledPoolThresholdStore Inner { get; set; } = inner;
    public ScheduledPoolThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledPoolThreshold? Find(string code)
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

    public void Save(ScheduledPoolThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledPoolThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled pool thresholds. Empty fields match all.</summary>
public sealed record ScheduledPoolThresholdQuery(string Title, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledPoolThreshold item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledPoolThreshold> Apply(List<ScheduledPoolThreshold> items)
    {
        var matched = Collections.Filter(items, (ScheduledPoolThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled pool threshold store.</summary>
public static class ScheduledPoolThresholdStore
{
    public static IScheduledPoolThresholdStore NewMemoryScheduledPoolThresholdStore()
    {
        return new MemoryScheduledPoolThresholdStore(
            new Dictionary<string, ScheduledPoolThreshold>(),
            new List<string>());
    }

    public static IScheduledPoolThresholdStore NewCachedScheduledPoolThresholdStore(MemoryScheduledPoolThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledPoolThresholdStore(
            inner,
            ScheduledPoolThresholdCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledPoolThreshold> FindScheduledPoolThresholds(IScheduledPoolThresholdStore store, ScheduledPoolThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
