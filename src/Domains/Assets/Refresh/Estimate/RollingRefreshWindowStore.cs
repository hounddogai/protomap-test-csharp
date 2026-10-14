namespace Acme.Workforce.Assets.Refresh.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling refresh windows by code.</summary>
public interface IRollingRefreshWindowStore
{
    RollingRefreshWindow? Find(string code);
    void Save(RollingRefreshWindow item);
    bool Remove(string code);
    List<RollingRefreshWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingRefreshWindowStore(Dictionary<string, RollingRefreshWindow> items, List<string> order) : IRollingRefreshWindowStore
{
    public Dictionary<string, RollingRefreshWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingRefreshWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingRefreshWindow item)
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

    public List<RollingRefreshWindow> List()
    {
        var outValue = new List<RollingRefreshWindow>();
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
public sealed class CachedRollingRefreshWindowStore(MemoryRollingRefreshWindowStore inner, RollingRefreshWindowCache cache, int hits, int misses) : IRollingRefreshWindowStore
{
    public MemoryRollingRefreshWindowStore Inner { get; set; } = inner;
    public RollingRefreshWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingRefreshWindow? Find(string code)
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

    public void Save(RollingRefreshWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingRefreshWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling refresh windows. Empty fields match all.</summary>
public sealed record RollingRefreshWindowQuery(string Region, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(RollingRefreshWindow item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingRefreshWindow> Apply(List<RollingRefreshWindow> items)
    {
        var matched = Collections.Filter(items, (RollingRefreshWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling refresh window store.</summary>
public static class RollingRefreshWindowStore
{
    public static IRollingRefreshWindowStore NewMemoryRollingRefreshWindowStore()
    {
        return new MemoryRollingRefreshWindowStore(
            new Dictionary<string, RollingRefreshWindow>(),
            new List<string>());
    }

    public static IRollingRefreshWindowStore NewCachedRollingRefreshWindowStore(MemoryRollingRefreshWindowStore inner, int ttl, IClock clock)
    {
        return new CachedRollingRefreshWindowStore(inner, RollingRefreshWindowCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<RollingRefreshWindow> FindRollingRefreshWindows(IRollingRefreshWindowStore store, RollingRefreshWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
