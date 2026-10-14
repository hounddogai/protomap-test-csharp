namespace Acme.Workforce.Engagement.TownHall.HallException;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps town hall preferences by code.</summary>
public interface ITownHallPreferenceStore
{
    TownHallPreference? Find(string code);
    void Save(TownHallPreference item);
    bool Remove(string code);
    List<TownHallPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTownHallPreferenceStore(Dictionary<string, TownHallPreference> items, List<string> order) : ITownHallPreferenceStore
{
    public Dictionary<string, TownHallPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TownHallPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TownHallPreference item)
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

    public List<TownHallPreference> List()
    {
        var outValue = new List<TownHallPreference>();
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
public sealed class CachedTownHallPreferenceStore(MemoryTownHallPreferenceStore inner, TownHallPreferenceCache cache, int hits, int misses) : ITownHallPreferenceStore
{
    public MemoryTownHallPreferenceStore Inner { get; set; } = inner;
    public TownHallPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TownHallPreference? Find(string code)
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

    public void Save(TownHallPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TownHallPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects town hall preferences. Empty fields match all.</summary>
public sealed record TownHallPreferenceQuery(string Label, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(TownHallPreference item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<TownHallPreference> Apply(List<TownHallPreference> items)
    {
        var matched = Collections.Filter(items, (TownHallPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of town hall preference store.</summary>
public static class TownHallPreferenceStore
{
    public static ITownHallPreferenceStore NewMemoryTownHallPreferenceStore()
    {
        return new MemoryTownHallPreferenceStore(new Dictionary<string, TownHallPreference>(), new List<string>());
    }

    public static ITownHallPreferenceStore NewCachedTownHallPreferenceStore(MemoryTownHallPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedTownHallPreferenceStore(inner, TownHallPreferenceCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<TownHallPreference> FindTownHallPreferences(ITownHallPreferenceStore store, TownHallPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
