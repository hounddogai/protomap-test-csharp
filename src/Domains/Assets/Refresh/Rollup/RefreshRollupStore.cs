namespace Acme.Workforce.Assets.Refresh.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps refresh rollups by code.</summary>
public interface IRefreshRollupStore
{
    RefreshRollup? Find(string code);
    void Save(RefreshRollup item);
    bool Remove(string code);
    List<RefreshRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRefreshRollupStore(Dictionary<string, RefreshRollup> items, List<string> order) : IRefreshRollupStore
{
    public Dictionary<string, RefreshRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RefreshRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RefreshRollup item)
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

    public List<RefreshRollup> List()
    {
        var outValue = new List<RefreshRollup>();
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
public sealed class CachedRefreshRollupStore(MemoryRefreshRollupStore inner, RefreshRollupCache cache, int hits, int misses) : IRefreshRollupStore
{
    public MemoryRefreshRollupStore Inner { get; set; } = inner;
    public RefreshRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RefreshRollup? Find(string code)
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

    public void Save(RefreshRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RefreshRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects refresh rollups. Empty fields match all.</summary>
public sealed record RefreshRollupQuery(string Team, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(RefreshRollup item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RefreshRollup> Apply(List<RefreshRollup> items)
    {
        var matched = Collections.Filter(items, (RefreshRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of refresh rollup store.</summary>
public static class RefreshRollupStore
{
    public static IRefreshRollupStore NewMemoryRefreshRollupStore()
    {
        return new MemoryRefreshRollupStore(new Dictionary<string, RefreshRollup>(), new List<string>());
    }

    public static IRefreshRollupStore NewCachedRefreshRollupStore(MemoryRefreshRollupStore inner, int ttl, IClock clock)
    {
        return new CachedRefreshRollupStore(inner, RefreshRollupCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<RefreshRollup> FindRefreshRollups(IRefreshRollupStore store, RefreshRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
