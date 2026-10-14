namespace Acme.Workforce.Talent.Pool.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pool preferences by code.</summary>
public interface IPoolPreferenceStore
{
    PoolPreference? Find(string code);
    void Save(PoolPreference item);
    bool Remove(string code);
    List<PoolPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPoolPreferenceStore(Dictionary<string, PoolPreference> items, List<string> order) : IPoolPreferenceStore
{
    public Dictionary<string, PoolPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PoolPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PoolPreference item)
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

    public List<PoolPreference> List()
    {
        var outValue = new List<PoolPreference>();
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
public sealed class CachedPoolPreferenceStore(MemoryPoolPreferenceStore inner, PoolPreferenceCache cache, int hits, int misses) : IPoolPreferenceStore
{
    public MemoryPoolPreferenceStore Inner { get; set; } = inner;
    public PoolPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PoolPreference? Find(string code)
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

    public void Save(PoolPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PoolPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pool preferences. Empty fields match all.</summary>
public sealed record PoolPreferenceQuery(string Country, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(PoolPreference item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PoolPreference> Apply(List<PoolPreference> items)
    {
        var matched = Collections.Filter(items, (PoolPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pool preference store.</summary>
public static class PoolPreferenceStore
{
    public static IPoolPreferenceStore NewMemoryPoolPreferenceStore()
    {
        return new MemoryPoolPreferenceStore(new Dictionary<string, PoolPreference>(), new List<string>());
    }

    public static IPoolPreferenceStore NewCachedPoolPreferenceStore(MemoryPoolPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedPoolPreferenceStore(inner, PoolPreferenceCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<PoolPreference> FindPoolPreferences(IPoolPreferenceStore store, PoolPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
