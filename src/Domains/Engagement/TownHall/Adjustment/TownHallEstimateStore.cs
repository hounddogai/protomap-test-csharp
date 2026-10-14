namespace Acme.Workforce.Engagement.TownHall.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps town hall estimates by code.</summary>
public interface ITownHallEstimateStore
{
    TownHallEstimate? Find(string code);
    void Save(TownHallEstimate item);
    bool Remove(string code);
    List<TownHallEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTownHallEstimateStore(Dictionary<string, TownHallEstimate> items, List<string> order) : ITownHallEstimateStore
{
    public Dictionary<string, TownHallEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TownHallEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TownHallEstimate item)
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

    public List<TownHallEstimate> List()
    {
        var outValue = new List<TownHallEstimate>();
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
public sealed class CachedTownHallEstimateStore(MemoryTownHallEstimateStore inner, TownHallEstimateCache cache, int hits, int misses) : ITownHallEstimateStore
{
    public MemoryTownHallEstimateStore Inner { get; set; } = inner;
    public TownHallEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TownHallEstimate? Find(string code)
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

    public void Save(TownHallEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TownHallEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects town hall estimates. Empty fields match all.</summary>
public sealed record TownHallEstimateQuery(string Name, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(TownHallEstimate item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TownHallEstimate> Apply(List<TownHallEstimate> items)
    {
        var matched = Collections.Filter(items, (TownHallEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of town hall estimate store.</summary>
public static class TownHallEstimateStore
{
    public static ITownHallEstimateStore NewMemoryTownHallEstimateStore()
    {
        return new MemoryTownHallEstimateStore(new Dictionary<string, TownHallEstimate>(), new List<string>());
    }

    public static ITownHallEstimateStore NewCachedTownHallEstimateStore(MemoryTownHallEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedTownHallEstimateStore(inner, TownHallEstimateCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<TownHallEstimate> FindTownHallEstimates(ITownHallEstimateStore store, TownHallEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
