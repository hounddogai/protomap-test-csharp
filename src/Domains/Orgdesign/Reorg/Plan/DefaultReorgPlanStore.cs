namespace Acme.Workforce.Orgdesign.Reorg.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default reorg plans by code.</summary>
public interface IDefaultReorgPlanStore
{
    DefaultReorgPlan? Find(string code);
    void Save(DefaultReorgPlan item);
    bool Remove(string code);
    List<DefaultReorgPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultReorgPlanStore(Dictionary<string, DefaultReorgPlan> items, List<string> order) : IDefaultReorgPlanStore
{
    public Dictionary<string, DefaultReorgPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultReorgPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultReorgPlan item)
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

    public List<DefaultReorgPlan> List()
    {
        var outValue = new List<DefaultReorgPlan>();
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
public sealed class CachedDefaultReorgPlanStore(MemoryDefaultReorgPlanStore inner, DefaultReorgPlanCache cache, int hits, int misses) : IDefaultReorgPlanStore
{
    public MemoryDefaultReorgPlanStore Inner { get; set; } = inner;
    public DefaultReorgPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultReorgPlan? Find(string code)
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

    public void Save(DefaultReorgPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultReorgPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default reorg plans. Empty fields match all.</summary>
public sealed record DefaultReorgPlanQuery(string Title, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultReorgPlan item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultReorgPlan> Apply(List<DefaultReorgPlan> items)
    {
        var matched = Collections.Filter(items, (DefaultReorgPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default reorg plan store.</summary>
public static class DefaultReorgPlanStore
{
    public static IDefaultReorgPlanStore NewMemoryDefaultReorgPlanStore()
    {
        return new MemoryDefaultReorgPlanStore(new Dictionary<string, DefaultReorgPlan>(), new List<string>());
    }

    public static IDefaultReorgPlanStore NewCachedDefaultReorgPlanStore(MemoryDefaultReorgPlanStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultReorgPlanStore(inner, DefaultReorgPlanCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<DefaultReorgPlan> FindDefaultReorgPlans(IDefaultReorgPlanStore store, DefaultReorgPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
