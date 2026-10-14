namespace Acme.Workforce.Assets.Refresh.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps refresh scores by code.</summary>
public interface IRefreshScoreStore
{
    RefreshScore? Find(string code);
    void Save(RefreshScore item);
    bool Remove(string code);
    List<RefreshScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRefreshScoreStore(Dictionary<string, RefreshScore> items, List<string> order) : IRefreshScoreStore
{
    public Dictionary<string, RefreshScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RefreshScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RefreshScore item)
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

    public List<RefreshScore> List()
    {
        var outValue = new List<RefreshScore>();
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
public sealed class CachedRefreshScoreStore(MemoryRefreshScoreStore inner, RefreshScoreCache cache, int hits, int misses) : IRefreshScoreStore
{
    public MemoryRefreshScoreStore Inner { get; set; } = inner;
    public RefreshScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RefreshScore? Find(string code)
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

    public void Save(RefreshScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RefreshScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects refresh scores. Empty fields match all.</summary>
public sealed record RefreshScoreQuery(string Category, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(RefreshScore item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<RefreshScore> Apply(List<RefreshScore> items)
    {
        var matched = Collections.Filter(items, (RefreshScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of refresh score store.</summary>
public static class RefreshScoreStore
{
    public static IRefreshScoreStore NewMemoryRefreshScoreStore()
    {
        return new MemoryRefreshScoreStore(new Dictionary<string, RefreshScore>(), new List<string>());
    }

    public static IRefreshScoreStore NewCachedRefreshScoreStore(MemoryRefreshScoreStore inner, int ttl, IClock clock)
    {
        return new CachedRefreshScoreStore(inner, RefreshScoreCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<RefreshScore> FindRefreshScores(IRefreshScoreStore store, RefreshScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
