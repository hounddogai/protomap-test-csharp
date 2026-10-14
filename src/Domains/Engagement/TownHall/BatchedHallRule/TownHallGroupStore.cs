namespace Acme.Workforce.Engagement.TownHall.BatchedHallRule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps town hall groups by code.</summary>
public interface ITownHallGroupStore
{
    TownHallGroup? Find(string code);
    void Save(TownHallGroup item);
    bool Remove(string code);
    List<TownHallGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTownHallGroupStore(Dictionary<string, TownHallGroup> items, List<string> order) : ITownHallGroupStore
{
    public Dictionary<string, TownHallGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TownHallGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TownHallGroup item)
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

    public List<TownHallGroup> List()
    {
        var outValue = new List<TownHallGroup>();
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
public sealed class CachedTownHallGroupStore(MemoryTownHallGroupStore inner, TownHallGroupCache cache, int hits, int misses) : ITownHallGroupStore
{
    public MemoryTownHallGroupStore Inner { get; set; } = inner;
    public TownHallGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TownHallGroup? Find(string code)
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

    public void Save(TownHallGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TownHallGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects town hall groups. Empty fields match all.</summary>
public sealed record TownHallGroupQuery(string Department, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(TownHallGroup item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TownHallGroup> Apply(List<TownHallGroup> items)
    {
        var matched = Collections.Filter(items, (TownHallGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of town hall group store.</summary>
public static class TownHallGroupStore
{
    public static ITownHallGroupStore NewMemoryTownHallGroupStore()
    {
        return new MemoryTownHallGroupStore(new Dictionary<string, TownHallGroup>(), new List<string>());
    }

    public static ITownHallGroupStore NewCachedTownHallGroupStore(MemoryTownHallGroupStore inner, int ttl, IClock clock)
    {
        return new CachedTownHallGroupStore(inner, TownHallGroupCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<TownHallGroup> FindTownHallGroups(ITownHallGroupStore store, TownHallGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
