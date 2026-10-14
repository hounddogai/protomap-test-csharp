namespace Acme.Workforce.Talent.Mobility.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled mobility rollups by code.</summary>
public interface IScheduledMobilityRollupStore
{
    ScheduledMobilityRollup? Find(string code);
    void Save(ScheduledMobilityRollup item);
    bool Remove(string code);
    List<ScheduledMobilityRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledMobilityRollupStore(Dictionary<string, ScheduledMobilityRollup> items, List<string> order) : IScheduledMobilityRollupStore
{
    public Dictionary<string, ScheduledMobilityRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledMobilityRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledMobilityRollup item)
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

    public List<ScheduledMobilityRollup> List()
    {
        var outValue = new List<ScheduledMobilityRollup>();
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
public sealed class CachedScheduledMobilityRollupStore(MemoryScheduledMobilityRollupStore inner, ScheduledMobilityRollupCache cache, int hits, int misses) : IScheduledMobilityRollupStore
{
    public MemoryScheduledMobilityRollupStore Inner { get; set; } = inner;
    public ScheduledMobilityRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledMobilityRollup? Find(string code)
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

    public void Save(ScheduledMobilityRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledMobilityRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled mobility rollups. Empty fields match all.</summary>
public sealed record ScheduledMobilityRollupQuery(string Title, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledMobilityRollup item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<ScheduledMobilityRollup> Apply(List<ScheduledMobilityRollup> items)
    {
        var matched = Collections.Filter(items, (ScheduledMobilityRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled mobility rollup store.</summary>
public static class ScheduledMobilityRollupStore
{
    public static IScheduledMobilityRollupStore NewMemoryScheduledMobilityRollupStore()
    {
        return new MemoryScheduledMobilityRollupStore(
            new Dictionary<string, ScheduledMobilityRollup>(),
            new List<string>());
    }

    public static IScheduledMobilityRollupStore NewCachedScheduledMobilityRollupStore(MemoryScheduledMobilityRollupStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledMobilityRollupStore(
            inner,
            ScheduledMobilityRollupCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledMobilityRollup> FindScheduledMobilityRollups(IScheduledMobilityRollupStore store, ScheduledMobilityRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
