namespace Acme.Workforce.Assets.Refresh.Forecast;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled refresh cycles by code.</summary>
public interface IScheduledRefreshCycleStore
{
    ScheduledRefreshCycle? Find(string code);
    void Save(ScheduledRefreshCycle item);
    bool Remove(string code);
    List<ScheduledRefreshCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledRefreshCycleStore(Dictionary<string, ScheduledRefreshCycle> items, List<string> order) : IScheduledRefreshCycleStore
{
    public Dictionary<string, ScheduledRefreshCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledRefreshCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledRefreshCycle item)
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

    public List<ScheduledRefreshCycle> List()
    {
        var outValue = new List<ScheduledRefreshCycle>();
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
public sealed class CachedScheduledRefreshCycleStore(MemoryScheduledRefreshCycleStore inner, ScheduledRefreshCycleCache cache, int hits, int misses) : IScheduledRefreshCycleStore
{
    public MemoryScheduledRefreshCycleStore Inner { get; set; } = inner;
    public ScheduledRefreshCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledRefreshCycle? Find(string code)
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

    public void Save(ScheduledRefreshCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledRefreshCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled refresh cycles. Empty fields match all.</summary>
public sealed record ScheduledRefreshCycleQuery(string Channel, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledRefreshCycle item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledRefreshCycle> Apply(List<ScheduledRefreshCycle> items)
    {
        var matched = Collections.Filter(items, (ScheduledRefreshCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled refresh cycle store.</summary>
public static class ScheduledRefreshCycleStore
{
    public static IScheduledRefreshCycleStore NewMemoryScheduledRefreshCycleStore()
    {
        return new MemoryScheduledRefreshCycleStore(
            new Dictionary<string, ScheduledRefreshCycle>(),
            new List<string>());
    }

    public static IScheduledRefreshCycleStore NewCachedScheduledRefreshCycleStore(MemoryScheduledRefreshCycleStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledRefreshCycleStore(inner, ScheduledRefreshCycleCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<ScheduledRefreshCycle> FindScheduledRefreshCycles(IScheduledRefreshCycleStore store, ScheduledRefreshCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
