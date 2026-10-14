namespace Acme.Workforce.Assets.Refresh.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered refresh scores by code.</summary>
public interface ITieredRefreshScoreStore
{
    TieredRefreshScore? Find(string code);
    void Save(TieredRefreshScore item);
    bool Remove(string code);
    List<TieredRefreshScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredRefreshScoreStore(Dictionary<string, TieredRefreshScore> items, List<string> order) : ITieredRefreshScoreStore
{
    public Dictionary<string, TieredRefreshScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredRefreshScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredRefreshScore item)
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

    public List<TieredRefreshScore> List()
    {
        var outValue = new List<TieredRefreshScore>();
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
public sealed class CachedTieredRefreshScoreStore(MemoryTieredRefreshScoreStore inner, TieredRefreshScoreCache cache, int hits, int misses) : ITieredRefreshScoreStore
{
    public MemoryTieredRefreshScoreStore Inner { get; set; } = inner;
    public TieredRefreshScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredRefreshScore? Find(string code)
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

    public void Save(TieredRefreshScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredRefreshScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered refresh scores. Empty fields match all.</summary>
public sealed record TieredRefreshScoreQuery(string Name, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(TieredRefreshScore item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredRefreshScore> Apply(List<TieredRefreshScore> items)
    {
        var matched = Collections.Filter(items, (TieredRefreshScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered refresh score store.</summary>
public static class TieredRefreshScoreStore
{
    public static ITieredRefreshScoreStore NewMemoryTieredRefreshScoreStore()
    {
        return new MemoryTieredRefreshScoreStore(new Dictionary<string, TieredRefreshScore>(), new List<string>());
    }

    public static ITieredRefreshScoreStore NewCachedTieredRefreshScoreStore(MemoryTieredRefreshScoreStore inner, int ttl, IClock clock)
    {
        return new CachedTieredRefreshScoreStore(inner, TieredRefreshScoreCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<TieredRefreshScore> FindTieredRefreshScores(ITieredRefreshScoreStore store, TieredRefreshScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
