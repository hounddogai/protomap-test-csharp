namespace Acme.Workforce.Engagement.TownHall.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps town hall entries by code.</summary>
public interface ITownHallEntryStore
{
    TownHallEntry? Find(string code);
    void Save(TownHallEntry item);
    bool Remove(string code);
    List<TownHallEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTownHallEntryStore(Dictionary<string, TownHallEntry> items, List<string> order) : ITownHallEntryStore
{
    public Dictionary<string, TownHallEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TownHallEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TownHallEntry item)
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

    public List<TownHallEntry> List()
    {
        var outValue = new List<TownHallEntry>();
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
public sealed class CachedTownHallEntryStore(MemoryTownHallEntryStore inner, TownHallEntryCache cache, int hits, int misses) : ITownHallEntryStore
{
    public MemoryTownHallEntryStore Inner { get; set; } = inner;
    public TownHallEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TownHallEntry? Find(string code)
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

    public void Save(TownHallEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TownHallEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects town hall entries. Empty fields match all.</summary>
public sealed record TownHallEntryQuery(string Category, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(TownHallEntry item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TownHallEntry> Apply(List<TownHallEntry> items)
    {
        var matched = Collections.Filter(items, (TownHallEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of town hall entry store.</summary>
public static class TownHallEntryStore
{
    public static ITownHallEntryStore NewMemoryTownHallEntryStore()
    {
        return new MemoryTownHallEntryStore(new Dictionary<string, TownHallEntry>(), new List<string>());
    }

    public static ITownHallEntryStore NewCachedTownHallEntryStore(MemoryTownHallEntryStore inner, int ttl, IClock clock)
    {
        return new CachedTownHallEntryStore(inner, TownHallEntryCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<TownHallEntry> FindTownHallEntries(ITownHallEntryStore store, TownHallEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
