namespace Acme.Workforce.Engagement.TownHall.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default town hall plans by code.</summary>
public interface IDefaultTownHallPlanStore
{
    DefaultTownHallPlan? Find(string code);
    void Save(DefaultTownHallPlan item);
    bool Remove(string code);
    List<DefaultTownHallPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultTownHallPlanStore(Dictionary<string, DefaultTownHallPlan> items, List<string> order) : IDefaultTownHallPlanStore
{
    public Dictionary<string, DefaultTownHallPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultTownHallPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultTownHallPlan item)
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

    public List<DefaultTownHallPlan> List()
    {
        var outValue = new List<DefaultTownHallPlan>();
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
public sealed class CachedDefaultTownHallPlanStore(MemoryDefaultTownHallPlanStore inner, DefaultTownHallPlanCache cache, int hits, int misses) : IDefaultTownHallPlanStore
{
    public MemoryDefaultTownHallPlanStore Inner { get; set; } = inner;
    public DefaultTownHallPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultTownHallPlan? Find(string code)
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

    public void Save(DefaultTownHallPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultTownHallPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default town hall plans. Empty fields match all.</summary>
public sealed record DefaultTownHallPlanQuery(string Country, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultTownHallPlan item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultTownHallPlan> Apply(List<DefaultTownHallPlan> items)
    {
        var matched = Collections.Filter(items, (DefaultTownHallPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default town hall plan store.</summary>
public static class DefaultTownHallPlanStore
{
    public static IDefaultTownHallPlanStore NewMemoryDefaultTownHallPlanStore()
    {
        return new MemoryDefaultTownHallPlanStore(new Dictionary<string, DefaultTownHallPlan>(), new List<string>());
    }

    public static IDefaultTownHallPlanStore NewCachedDefaultTownHallPlanStore(MemoryDefaultTownHallPlanStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultTownHallPlanStore(inner, DefaultTownHallPlanCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<DefaultTownHallPlan> FindDefaultTownHallPlans(IDefaultTownHallPlanStore store, DefaultTownHallPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
