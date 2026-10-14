namespace Acme.Workforce.Assets.Refresh.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling refresh rollups by code.</summary>
public interface IRollingRefreshRollupStore
{
    RollingRefreshRollup? Find(string code);
    void Save(RollingRefreshRollup item);
    bool Remove(string code);
    List<RollingRefreshRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingRefreshRollupStore(Dictionary<string, RollingRefreshRollup> items, List<string> order) : IRollingRefreshRollupStore
{
    public Dictionary<string, RollingRefreshRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingRefreshRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingRefreshRollup item)
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

    public List<RollingRefreshRollup> List()
    {
        var outValue = new List<RollingRefreshRollup>();
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
public sealed class CachedRollingRefreshRollupStore(MemoryRollingRefreshRollupStore inner, RollingRefreshRollupCache cache, int hits, int misses) : IRollingRefreshRollupStore
{
    public MemoryRollingRefreshRollupStore Inner { get; set; } = inner;
    public RollingRefreshRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingRefreshRollup? Find(string code)
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

    public void Save(RollingRefreshRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingRefreshRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling refresh rollups. Empty fields match all.</summary>
public sealed record RollingRefreshRollupQuery(string Unit, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(RollingRefreshRollup item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingRefreshRollup> Apply(List<RollingRefreshRollup> items)
    {
        var matched = Collections.Filter(items, (RollingRefreshRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling refresh rollup store.</summary>
public static class RollingRefreshRollupStore
{
    public static IRollingRefreshRollupStore NewMemoryRollingRefreshRollupStore()
    {
        return new MemoryRollingRefreshRollupStore(
            new Dictionary<string, RollingRefreshRollup>(),
            new List<string>());
    }

    public static IRollingRefreshRollupStore NewCachedRollingRefreshRollupStore(MemoryRollingRefreshRollupStore inner, int ttl, IClock clock)
    {
        return new CachedRollingRefreshRollupStore(inner, RollingRefreshRollupCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<RollingRefreshRollup> FindRollingRefreshRollups(IRollingRefreshRollupStore store, RollingRefreshRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
