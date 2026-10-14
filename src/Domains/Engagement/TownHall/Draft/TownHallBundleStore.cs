namespace Acme.Workforce.Engagement.TownHall.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps town hall bundles by code.</summary>
public interface ITownHallBundleStore
{
    TownHallBundle? Find(string code);
    void Save(TownHallBundle item);
    bool Remove(string code);
    List<TownHallBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTownHallBundleStore(Dictionary<string, TownHallBundle> items, List<string> order) : ITownHallBundleStore
{
    public Dictionary<string, TownHallBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TownHallBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TownHallBundle item)
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

    public List<TownHallBundle> List()
    {
        var outValue = new List<TownHallBundle>();
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
public sealed class CachedTownHallBundleStore(MemoryTownHallBundleStore inner, TownHallBundleCache cache, int hits, int misses) : ITownHallBundleStore
{
    public MemoryTownHallBundleStore Inner { get; set; } = inner;
    public TownHallBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TownHallBundle? Find(string code)
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

    public void Save(TownHallBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TownHallBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects town hall bundles. Empty fields match all.</summary>
public sealed record TownHallBundleQuery(string Segment, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(TownHallBundle item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TownHallBundle> Apply(List<TownHallBundle> items)
    {
        var matched = Collections.Filter(items, (TownHallBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of town hall bundle store.</summary>
public static class TownHallBundleStore
{
    public static ITownHallBundleStore NewMemoryTownHallBundleStore()
    {
        return new MemoryTownHallBundleStore(new Dictionary<string, TownHallBundle>(), new List<string>());
    }

    public static ITownHallBundleStore NewCachedTownHallBundleStore(MemoryTownHallBundleStore inner, int ttl, IClock clock)
    {
        return new CachedTownHallBundleStore(inner, TownHallBundleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<TownHallBundle> FindTownHallBundles(ITownHallBundleStore store, TownHallBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
