namespace Acme.Workforce.Workplace.FloorPlan.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default floor plan targets by code.</summary>
public interface IDefaultFloorPlanTargetStore
{
    DefaultFloorPlanTarget? Find(string code);
    void Save(DefaultFloorPlanTarget item);
    bool Remove(string code);
    List<DefaultFloorPlanTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultFloorPlanTargetStore(Dictionary<string, DefaultFloorPlanTarget> items, List<string> order) : IDefaultFloorPlanTargetStore
{
    public Dictionary<string, DefaultFloorPlanTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultFloorPlanTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultFloorPlanTarget item)
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

    public List<DefaultFloorPlanTarget> List()
    {
        var outValue = new List<DefaultFloorPlanTarget>();
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
public sealed class CachedDefaultFloorPlanTargetStore(MemoryDefaultFloorPlanTargetStore inner, DefaultFloorPlanTargetCache cache, int hits, int misses) : IDefaultFloorPlanTargetStore
{
    public MemoryDefaultFloorPlanTargetStore Inner { get; set; } = inner;
    public DefaultFloorPlanTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultFloorPlanTarget? Find(string code)
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

    public void Save(DefaultFloorPlanTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultFloorPlanTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default floor plan targets. Empty fields match all.</summary>
public sealed record DefaultFloorPlanTargetQuery(string Label, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultFloorPlanTarget item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<DefaultFloorPlanTarget> Apply(List<DefaultFloorPlanTarget> items)
    {
        var matched = Collections.Filter(items, (DefaultFloorPlanTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default floor plan target store.</summary>
public static class DefaultFloorPlanTargetStore
{
    public static IDefaultFloorPlanTargetStore NewMemoryDefaultFloorPlanTargetStore()
    {
        return new MemoryDefaultFloorPlanTargetStore(
            new Dictionary<string, DefaultFloorPlanTarget>(),
            new List<string>());
    }

    public static IDefaultFloorPlanTargetStore NewCachedDefaultFloorPlanTargetStore(MemoryDefaultFloorPlanTargetStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultFloorPlanTargetStore(
            inner,
            DefaultFloorPlanTargetCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<DefaultFloorPlanTarget> FindDefaultFloorPlanTargets(IDefaultFloorPlanTargetStore store, DefaultFloorPlanTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
