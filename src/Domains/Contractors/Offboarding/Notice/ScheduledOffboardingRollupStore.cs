namespace Acme.Workforce.Contractors.Offboarding.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled offboarding rollups by code.</summary>
public interface IScheduledOffboardingRollupStore
{
    ScheduledOffboardingRollup? Find(string code);
    void Save(ScheduledOffboardingRollup item);
    bool Remove(string code);
    List<ScheduledOffboardingRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledOffboardingRollupStore(Dictionary<string, ScheduledOffboardingRollup> items, List<string> order) : IScheduledOffboardingRollupStore
{
    public Dictionary<string, ScheduledOffboardingRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledOffboardingRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledOffboardingRollup item)
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

    public List<ScheduledOffboardingRollup> List()
    {
        var outValue = new List<ScheduledOffboardingRollup>();
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
public sealed class CachedScheduledOffboardingRollupStore(MemoryScheduledOffboardingRollupStore inner, ScheduledOffboardingRollupCache cache, int hits, int misses) : IScheduledOffboardingRollupStore
{
    public MemoryScheduledOffboardingRollupStore Inner { get; set; } = inner;
    public ScheduledOffboardingRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledOffboardingRollup? Find(string code)
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

    public void Save(ScheduledOffboardingRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledOffboardingRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled offboarding rollups. Empty fields match all.</summary>
public sealed record ScheduledOffboardingRollupQuery(string Team, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledOffboardingRollup item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledOffboardingRollup> Apply(List<ScheduledOffboardingRollup> items)
    {
        var matched = Collections.Filter(items, (ScheduledOffboardingRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled offboarding rollup store.</summary>
public static class ScheduledOffboardingRollupStore
{
    public static IScheduledOffboardingRollupStore NewMemoryScheduledOffboardingRollupStore()
    {
        return new MemoryScheduledOffboardingRollupStore(
            new Dictionary<string, ScheduledOffboardingRollup>(),
            new List<string>());
    }

    public static IScheduledOffboardingRollupStore NewCachedScheduledOffboardingRollupStore(MemoryScheduledOffboardingRollupStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledOffboardingRollupStore(
            inner,
            ScheduledOffboardingRollupCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledOffboardingRollup> FindScheduledOffboardingRollups(IScheduledOffboardingRollupStore store, ScheduledOffboardingRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
