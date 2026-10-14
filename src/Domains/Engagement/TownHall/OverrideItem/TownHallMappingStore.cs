namespace Acme.Workforce.Engagement.TownHall.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps town hall mappings by code.</summary>
public interface ITownHallMappingStore
{
    TownHallMapping? Find(string code);
    void Save(TownHallMapping item);
    bool Remove(string code);
    List<TownHallMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTownHallMappingStore(Dictionary<string, TownHallMapping> items, List<string> order) : ITownHallMappingStore
{
    public Dictionary<string, TownHallMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TownHallMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TownHallMapping item)
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

    public List<TownHallMapping> List()
    {
        var outValue = new List<TownHallMapping>();
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
public sealed class CachedTownHallMappingStore(MemoryTownHallMappingStore inner, TownHallMappingCache cache, int hits, int misses) : ITownHallMappingStore
{
    public MemoryTownHallMappingStore Inner { get; set; } = inner;
    public TownHallMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TownHallMapping? Find(string code)
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

    public void Save(TownHallMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TownHallMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects town hall mappings. Empty fields match all.</summary>
public sealed record TownHallMappingQuery(string Region, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(TownHallMapping item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TownHallMapping> Apply(List<TownHallMapping> items)
    {
        var matched = Collections.Filter(items, (TownHallMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of town hall mapping store.</summary>
public static class TownHallMappingStore
{
    public static ITownHallMappingStore NewMemoryTownHallMappingStore()
    {
        return new MemoryTownHallMappingStore(new Dictionary<string, TownHallMapping>(), new List<string>());
    }

    public static ITownHallMappingStore NewCachedTownHallMappingStore(MemoryTownHallMappingStore inner, int ttl, IClock clock)
    {
        return new CachedTownHallMappingStore(inner, TownHallMappingCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<TownHallMapping> FindTownHallMappings(ITownHallMappingStore store, TownHallMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
