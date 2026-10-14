namespace Acme.Workforce.Workplace.AccessBadge.Manifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global access badge cycles by code.</summary>
public interface IGlobalAccessBadgeCycleStore
{
    GlobalAccessBadgeCycle? Find(string code);
    void Save(GlobalAccessBadgeCycle item);
    bool Remove(string code);
    List<GlobalAccessBadgeCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalAccessBadgeCycleStore(Dictionary<string, GlobalAccessBadgeCycle> items, List<string> order) : IGlobalAccessBadgeCycleStore
{
    public Dictionary<string, GlobalAccessBadgeCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalAccessBadgeCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalAccessBadgeCycle item)
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

    public List<GlobalAccessBadgeCycle> List()
    {
        var outValue = new List<GlobalAccessBadgeCycle>();
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
public sealed class CachedGlobalAccessBadgeCycleStore(MemoryGlobalAccessBadgeCycleStore inner, GlobalAccessBadgeCycleCache cache, int hits, int misses) : IGlobalAccessBadgeCycleStore
{
    public MemoryGlobalAccessBadgeCycleStore Inner { get; set; } = inner;
    public GlobalAccessBadgeCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalAccessBadgeCycle? Find(string code)
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

    public void Save(GlobalAccessBadgeCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalAccessBadgeCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global access badge cycles. Empty fields match all.</summary>
public sealed record GlobalAccessBadgeCycleQuery(string Team, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalAccessBadgeCycle item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalAccessBadgeCycle> Apply(List<GlobalAccessBadgeCycle> items)
    {
        var matched = Collections.Filter(items, (GlobalAccessBadgeCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global access badge cycle store.</summary>
public static class GlobalAccessBadgeCycleStore
{
    public static IGlobalAccessBadgeCycleStore NewMemoryGlobalAccessBadgeCycleStore()
    {
        return new MemoryGlobalAccessBadgeCycleStore(
            new Dictionary<string, GlobalAccessBadgeCycle>(),
            new List<string>());
    }

    public static IGlobalAccessBadgeCycleStore NewCachedGlobalAccessBadgeCycleStore(MemoryGlobalAccessBadgeCycleStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalAccessBadgeCycleStore(
            inner,
            GlobalAccessBadgeCycleCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<GlobalAccessBadgeCycle> FindGlobalAccessBadgeCycles(IGlobalAccessBadgeCycleStore store, GlobalAccessBadgeCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
