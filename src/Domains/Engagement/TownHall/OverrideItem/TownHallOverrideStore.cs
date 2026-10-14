namespace Acme.Workforce.Engagement.TownHall.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps town hall overrides by code.</summary>
public interface ITownHallOverrideStore
{
    TownHallOverride? Find(string code);
    void Save(TownHallOverride item);
    bool Remove(string code);
    List<TownHallOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTownHallOverrideStore(Dictionary<string, TownHallOverride> items, List<string> order) : ITownHallOverrideStore
{
    public Dictionary<string, TownHallOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TownHallOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TownHallOverride item)
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

    public List<TownHallOverride> List()
    {
        var outValue = new List<TownHallOverride>();
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
public sealed class CachedTownHallOverrideStore(MemoryTownHallOverrideStore inner, TownHallOverrideCache cache, int hits, int misses) : ITownHallOverrideStore
{
    public MemoryTownHallOverrideStore Inner { get; set; } = inner;
    public TownHallOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TownHallOverride? Find(string code)
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

    public void Save(TownHallOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TownHallOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects town hall overrides. Empty fields match all.</summary>
public sealed record TownHallOverrideQuery(string Title, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(TownHallOverride item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TownHallOverride> Apply(List<TownHallOverride> items)
    {
        var matched = Collections.Filter(items, (TownHallOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of town hall override store.</summary>
public static class TownHallOverrideStore
{
    public static ITownHallOverrideStore NewMemoryTownHallOverrideStore()
    {
        return new MemoryTownHallOverrideStore(new Dictionary<string, TownHallOverride>(), new List<string>());
    }

    public static ITownHallOverrideStore NewCachedTownHallOverrideStore(MemoryTownHallOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedTownHallOverrideStore(inner, TownHallOverrideCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<TownHallOverride> FindTownHallOverrides(ITownHallOverrideStore store, TownHallOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
