namespace Acme.Workforce.Engagement.TownHall.HallException;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps town hall scores by code.</summary>
public interface ITownHallScoreStore
{
    TownHallScore? Find(string code);
    void Save(TownHallScore item);
    bool Remove(string code);
    List<TownHallScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTownHallScoreStore(Dictionary<string, TownHallScore> items, List<string> order) : ITownHallScoreStore
{
    public Dictionary<string, TownHallScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TownHallScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TownHallScore item)
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

    public List<TownHallScore> List()
    {
        var outValue = new List<TownHallScore>();
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
public sealed class CachedTownHallScoreStore(MemoryTownHallScoreStore inner, TownHallScoreCache cache, int hits, int misses) : ITownHallScoreStore
{
    public MemoryTownHallScoreStore Inner { get; set; } = inner;
    public TownHallScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TownHallScore? Find(string code)
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

    public void Save(TownHallScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TownHallScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects town hall scores. Empty fields match all.</summary>
public sealed record TownHallScoreQuery(string Name, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(TownHallScore item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TownHallScore> Apply(List<TownHallScore> items)
    {
        var matched = Collections.Filter(items, (TownHallScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of town hall score store.</summary>
public static class TownHallScoreStore
{
    public static ITownHallScoreStore NewMemoryTownHallScoreStore()
    {
        return new MemoryTownHallScoreStore(new Dictionary<string, TownHallScore>(), new List<string>());
    }

    public static ITownHallScoreStore NewCachedTownHallScoreStore(MemoryTownHallScoreStore inner, int ttl, IClock clock)
    {
        return new CachedTownHallScoreStore(inner, TownHallScoreCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<TownHallScore> FindTownHallScores(ITownHallScoreStore store, TownHallScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
