namespace Acme.Workforce.Engagement.TownHall.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps town hall windows by code.</summary>
public interface ITownHallWindowStore
{
    TownHallWindow? Find(string code);
    void Save(TownHallWindow item);
    bool Remove(string code);
    List<TownHallWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTownHallWindowStore(Dictionary<string, TownHallWindow> items, List<string> order) : ITownHallWindowStore
{
    public Dictionary<string, TownHallWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TownHallWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TownHallWindow item)
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

    public List<TownHallWindow> List()
    {
        var outValue = new List<TownHallWindow>();
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
public sealed class CachedTownHallWindowStore(MemoryTownHallWindowStore inner, TownHallWindowCache cache, int hits, int misses) : ITownHallWindowStore
{
    public MemoryTownHallWindowStore Inner { get; set; } = inner;
    public TownHallWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TownHallWindow? Find(string code)
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

    public void Save(TownHallWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TownHallWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects town hall windows. Empty fields match all.</summary>
public sealed record TownHallWindowQuery(string Segment, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(TownHallWindow item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TownHallWindow> Apply(List<TownHallWindow> items)
    {
        var matched = Collections.Filter(items, (TownHallWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of town hall window store.</summary>
public static class TownHallWindowStore
{
    public static ITownHallWindowStore NewMemoryTownHallWindowStore()
    {
        return new MemoryTownHallWindowStore(new Dictionary<string, TownHallWindow>(), new List<string>());
    }

    public static ITownHallWindowStore NewCachedTownHallWindowStore(MemoryTownHallWindowStore inner, int ttl, IClock clock)
    {
        return new CachedTownHallWindowStore(inner, TownHallWindowCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<TownHallWindow> FindTownHallWindows(ITownHallWindowStore store, TownHallWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
