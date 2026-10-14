namespace Acme.Workforce.Talent.Pool.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pool baselines by code.</summary>
public interface IPoolBaselineStore
{
    PoolBaseline? Find(string code);
    void Save(PoolBaseline item);
    bool Remove(string code);
    List<PoolBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPoolBaselineStore(Dictionary<string, PoolBaseline> items, List<string> order) : IPoolBaselineStore
{
    public Dictionary<string, PoolBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PoolBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PoolBaseline item)
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

    public List<PoolBaseline> List()
    {
        var outValue = new List<PoolBaseline>();
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
public sealed class CachedPoolBaselineStore(MemoryPoolBaselineStore inner, PoolBaselineCache cache, int hits, int misses) : IPoolBaselineStore
{
    public MemoryPoolBaselineStore Inner { get; set; } = inner;
    public PoolBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PoolBaseline? Find(string code)
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

    public void Save(PoolBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PoolBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pool baselines. Empty fields match all.</summary>
public sealed record PoolBaselineQuery(string Name, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(PoolBaseline item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PoolBaseline> Apply(List<PoolBaseline> items)
    {
        var matched = Collections.Filter(items, (PoolBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pool baseline store.</summary>
public static class PoolBaselineStore
{
    public static IPoolBaselineStore NewMemoryPoolBaselineStore()
    {
        return new MemoryPoolBaselineStore(new Dictionary<string, PoolBaseline>(), new List<string>());
    }

    public static IPoolBaselineStore NewCachedPoolBaselineStore(MemoryPoolBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedPoolBaselineStore(inner, PoolBaselineCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<PoolBaseline> FindPoolBaselines(IPoolBaselineStore store, PoolBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
