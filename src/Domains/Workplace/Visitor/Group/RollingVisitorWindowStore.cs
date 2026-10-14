namespace Acme.Workforce.Workplace.Visitor.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling visitor windows by code.</summary>
public interface IRollingVisitorWindowStore
{
    RollingVisitorWindow? Find(string code);
    void Save(RollingVisitorWindow item);
    bool Remove(string code);
    List<RollingVisitorWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingVisitorWindowStore(Dictionary<string, RollingVisitorWindow> items, List<string> order) : IRollingVisitorWindowStore
{
    public Dictionary<string, RollingVisitorWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingVisitorWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingVisitorWindow item)
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

    public List<RollingVisitorWindow> List()
    {
        var outValue = new List<RollingVisitorWindow>();
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
public sealed class CachedRollingVisitorWindowStore(MemoryRollingVisitorWindowStore inner, RollingVisitorWindowCache cache, int hits, int misses) : IRollingVisitorWindowStore
{
    public MemoryRollingVisitorWindowStore Inner { get; set; } = inner;
    public RollingVisitorWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingVisitorWindow? Find(string code)
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

    public void Save(RollingVisitorWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingVisitorWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling visitor windows. Empty fields match all.</summary>
public sealed record RollingVisitorWindowQuery(string Channel, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(RollingVisitorWindow item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingVisitorWindow> Apply(List<RollingVisitorWindow> items)
    {
        var matched = Collections.Filter(items, (RollingVisitorWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling visitor window store.</summary>
public static class RollingVisitorWindowStore
{
    public static IRollingVisitorWindowStore NewMemoryRollingVisitorWindowStore()
    {
        return new MemoryRollingVisitorWindowStore(
            new Dictionary<string, RollingVisitorWindow>(),
            new List<string>());
    }

    public static IRollingVisitorWindowStore NewCachedRollingVisitorWindowStore(MemoryRollingVisitorWindowStore inner, int ttl, IClock clock)
    {
        return new CachedRollingVisitorWindowStore(inner, RollingVisitorWindowCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<RollingVisitorWindow> FindRollingVisitorWindows(IRollingVisitorWindowStore store, RollingVisitorWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
