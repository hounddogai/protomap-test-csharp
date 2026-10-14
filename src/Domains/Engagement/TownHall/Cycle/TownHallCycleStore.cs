namespace Acme.Workforce.Engagement.TownHall.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps town hall cycles by code.</summary>
public interface ITownHallCycleStore
{
    TownHallCycle? Find(string code);
    void Save(TownHallCycle item);
    bool Remove(string code);
    List<TownHallCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTownHallCycleStore(Dictionary<string, TownHallCycle> items, List<string> order) : ITownHallCycleStore
{
    public Dictionary<string, TownHallCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TownHallCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TownHallCycle item)
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

    public List<TownHallCycle> List()
    {
        var outValue = new List<TownHallCycle>();
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
public sealed class CachedTownHallCycleStore(MemoryTownHallCycleStore inner, TownHallCycleCache cache, int hits, int misses) : ITownHallCycleStore
{
    public MemoryTownHallCycleStore Inner { get; set; } = inner;
    public TownHallCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TownHallCycle? Find(string code)
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

    public void Save(TownHallCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TownHallCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects town hall cycles. Empty fields match all.</summary>
public sealed record TownHallCycleQuery(string Unit, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(TownHallCycle item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TownHallCycle> Apply(List<TownHallCycle> items)
    {
        var matched = Collections.Filter(items, (TownHallCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of town hall cycle store.</summary>
public static class TownHallCycleStore
{
    public static ITownHallCycleStore NewMemoryTownHallCycleStore()
    {
        return new MemoryTownHallCycleStore(new Dictionary<string, TownHallCycle>(), new List<string>());
    }

    public static ITownHallCycleStore NewCachedTownHallCycleStore(MemoryTownHallCycleStore inner, int ttl, IClock clock)
    {
        return new CachedTownHallCycleStore(inner, TownHallCycleCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<TownHallCycle> FindTownHallCycles(ITownHallCycleStore store, TownHallCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
