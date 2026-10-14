namespace Acme.Workforce.Absence.Balance.BalanceGroup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled balance groups by code.</summary>
public interface IScheduledBalanceGroupStore
{
    ScheduledBalanceGroup? Find(string code);
    void Save(ScheduledBalanceGroup item);
    bool Remove(string code);
    List<ScheduledBalanceGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledBalanceGroupStore(Dictionary<string, ScheduledBalanceGroup> items, List<string> order) : IScheduledBalanceGroupStore
{
    public Dictionary<string, ScheduledBalanceGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledBalanceGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledBalanceGroup item)
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

    public List<ScheduledBalanceGroup> List()
    {
        var outValue = new List<ScheduledBalanceGroup>();
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
public sealed class CachedScheduledBalanceGroupStore(MemoryScheduledBalanceGroupStore inner, ScheduledBalanceGroupCache cache, int hits, int misses) : IScheduledBalanceGroupStore
{
    public MemoryScheduledBalanceGroupStore Inner { get; set; } = inner;
    public ScheduledBalanceGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledBalanceGroup? Find(string code)
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

    public void Save(ScheduledBalanceGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledBalanceGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled balance groups. Empty fields match all.</summary>
public sealed record ScheduledBalanceGroupQuery(string Department, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledBalanceGroup item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<ScheduledBalanceGroup> Apply(List<ScheduledBalanceGroup> items)
    {
        var matched = Collections.Filter(items, (ScheduledBalanceGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled balance group store.</summary>
public static class ScheduledBalanceGroupStore
{
    public static IScheduledBalanceGroupStore NewMemoryScheduledBalanceGroupStore()
    {
        return new MemoryScheduledBalanceGroupStore(
            new Dictionary<string, ScheduledBalanceGroup>(),
            new List<string>());
    }

    public static IScheduledBalanceGroupStore NewCachedScheduledBalanceGroupStore(MemoryScheduledBalanceGroupStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledBalanceGroupStore(inner, ScheduledBalanceGroupCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<ScheduledBalanceGroup> FindScheduledBalanceGroups(IScheduledBalanceGroupStore store, ScheduledBalanceGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
