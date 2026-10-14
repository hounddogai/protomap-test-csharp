namespace Acme.Workforce.Engagement.ActionPlan.Claim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default action plan rollups by code.</summary>
public interface IDefaultActionPlanRollupStore
{
    DefaultActionPlanRollup? Find(string code);
    void Save(DefaultActionPlanRollup item);
    bool Remove(string code);
    List<DefaultActionPlanRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultActionPlanRollupStore(Dictionary<string, DefaultActionPlanRollup> items, List<string> order) : IDefaultActionPlanRollupStore
{
    public Dictionary<string, DefaultActionPlanRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultActionPlanRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultActionPlanRollup item)
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

    public List<DefaultActionPlanRollup> List()
    {
        var outValue = new List<DefaultActionPlanRollup>();
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
public sealed class CachedDefaultActionPlanRollupStore(MemoryDefaultActionPlanRollupStore inner, DefaultActionPlanRollupCache cache, int hits, int misses) : IDefaultActionPlanRollupStore
{
    public MemoryDefaultActionPlanRollupStore Inner { get; set; } = inner;
    public DefaultActionPlanRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultActionPlanRollup? Find(string code)
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

    public void Save(DefaultActionPlanRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultActionPlanRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default action plan rollups. Empty fields match all.</summary>
public sealed record DefaultActionPlanRollupQuery(string Team, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultActionPlanRollup item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultActionPlanRollup> Apply(List<DefaultActionPlanRollup> items)
    {
        var matched = Collections.Filter(items, (DefaultActionPlanRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default action plan rollup store.</summary>
public static class DefaultActionPlanRollupStore
{
    public static IDefaultActionPlanRollupStore NewMemoryDefaultActionPlanRollupStore()
    {
        return new MemoryDefaultActionPlanRollupStore(
            new Dictionary<string, DefaultActionPlanRollup>(),
            new List<string>());
    }

    public static IDefaultActionPlanRollupStore NewCachedDefaultActionPlanRollupStore(MemoryDefaultActionPlanRollupStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultActionPlanRollupStore(
            inner,
            DefaultActionPlanRollupCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<DefaultActionPlanRollup> FindDefaultActionPlanRollups(IDefaultActionPlanRollupStore store, DefaultActionPlanRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
