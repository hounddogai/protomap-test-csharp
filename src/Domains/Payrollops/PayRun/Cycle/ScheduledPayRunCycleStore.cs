namespace Acme.Workforce.Payrollops.PayRun.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled pay run cycles by code.</summary>
public interface IScheduledPayRunCycleStore
{
    ScheduledPayRunCycle? Find(string code);
    void Save(ScheduledPayRunCycle item);
    bool Remove(string code);
    List<ScheduledPayRunCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledPayRunCycleStore(Dictionary<string, ScheduledPayRunCycle> items, List<string> order) : IScheduledPayRunCycleStore
{
    public Dictionary<string, ScheduledPayRunCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledPayRunCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledPayRunCycle item)
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

    public List<ScheduledPayRunCycle> List()
    {
        var outValue = new List<ScheduledPayRunCycle>();
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
public sealed class CachedScheduledPayRunCycleStore(MemoryScheduledPayRunCycleStore inner, ScheduledPayRunCycleCache cache, int hits, int misses) : IScheduledPayRunCycleStore
{
    public MemoryScheduledPayRunCycleStore Inner { get; set; } = inner;
    public ScheduledPayRunCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledPayRunCycle? Find(string code)
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

    public void Save(ScheduledPayRunCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledPayRunCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled pay run cycles. Empty fields match all.</summary>
public sealed record ScheduledPayRunCycleQuery(string Team, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledPayRunCycle item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledPayRunCycle> Apply(List<ScheduledPayRunCycle> items)
    {
        var matched = Collections.Filter(items, (ScheduledPayRunCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled pay run cycle store.</summary>
public static class ScheduledPayRunCycleStore
{
    public static IScheduledPayRunCycleStore NewMemoryScheduledPayRunCycleStore()
    {
        return new MemoryScheduledPayRunCycleStore(
            new Dictionary<string, ScheduledPayRunCycle>(),
            new List<string>());
    }

    public static IScheduledPayRunCycleStore NewCachedScheduledPayRunCycleStore(MemoryScheduledPayRunCycleStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledPayRunCycleStore(inner, ScheduledPayRunCycleCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<ScheduledPayRunCycle> FindScheduledPayRunCycles(IScheduledPayRunCycleStore store, ScheduledPayRunCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
