namespace Acme.Workforce.Orgdesign.Reorg.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling reorg targets by code.</summary>
public interface IRollingReorgTargetStore
{
    RollingReorgTarget? Find(string code);
    void Save(RollingReorgTarget item);
    bool Remove(string code);
    List<RollingReorgTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingReorgTargetStore(Dictionary<string, RollingReorgTarget> items, List<string> order) : IRollingReorgTargetStore
{
    public Dictionary<string, RollingReorgTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingReorgTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingReorgTarget item)
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

    public List<RollingReorgTarget> List()
    {
        var outValue = new List<RollingReorgTarget>();
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
public sealed class CachedRollingReorgTargetStore(MemoryRollingReorgTargetStore inner, RollingReorgTargetCache cache, int hits, int misses) : IRollingReorgTargetStore
{
    public MemoryRollingReorgTargetStore Inner { get; set; } = inner;
    public RollingReorgTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingReorgTarget? Find(string code)
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

    public void Save(RollingReorgTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingReorgTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling reorg targets. Empty fields match all.</summary>
public sealed record RollingReorgTargetQuery(string Name, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(RollingReorgTarget item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<RollingReorgTarget> Apply(List<RollingReorgTarget> items)
    {
        var matched = Collections.Filter(items, (RollingReorgTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling reorg target store.</summary>
public static class RollingReorgTargetStore
{
    public static IRollingReorgTargetStore NewMemoryRollingReorgTargetStore()
    {
        return new MemoryRollingReorgTargetStore(new Dictionary<string, RollingReorgTarget>(), new List<string>());
    }

    public static IRollingReorgTargetStore NewCachedRollingReorgTargetStore(MemoryRollingReorgTargetStore inner, int ttl, IClock clock)
    {
        return new CachedRollingReorgTargetStore(inner, RollingReorgTargetCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<RollingReorgTarget> FindRollingReorgTargets(IRollingReorgTargetStore store, RollingReorgTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
