namespace Acme.Workforce.Talent.Pool.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pool profiles by code.</summary>
public interface IPoolProfileStore
{
    PoolProfile? Find(string code);
    void Save(PoolProfile item);
    bool Remove(string code);
    List<PoolProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPoolProfileStore(Dictionary<string, PoolProfile> items, List<string> order) : IPoolProfileStore
{
    public Dictionary<string, PoolProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PoolProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PoolProfile item)
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

    public List<PoolProfile> List()
    {
        var outValue = new List<PoolProfile>();
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
public sealed class CachedPoolProfileStore(MemoryPoolProfileStore inner, PoolProfileCache cache, int hits, int misses) : IPoolProfileStore
{
    public MemoryPoolProfileStore Inner { get; set; } = inner;
    public PoolProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PoolProfile? Find(string code)
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

    public void Save(PoolProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PoolProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pool profiles. Empty fields match all.</summary>
public sealed record PoolProfileQuery(string Source, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(PoolProfile item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PoolProfile> Apply(List<PoolProfile> items)
    {
        var matched = Collections.Filter(items, (PoolProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pool profile store.</summary>
public static class PoolProfileStore
{
    public static IPoolProfileStore NewMemoryPoolProfileStore()
    {
        return new MemoryPoolProfileStore(new Dictionary<string, PoolProfile>(), new List<string>());
    }

    public static IPoolProfileStore NewCachedPoolProfileStore(MemoryPoolProfileStore inner, int ttl, IClock clock)
    {
        return new CachedPoolProfileStore(inner, PoolProfileCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PoolProfile> FindPoolProfiles(IPoolProfileStore store, PoolProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
