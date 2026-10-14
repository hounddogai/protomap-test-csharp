namespace Acme.Workforce.Contractors.Rate.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default rate cycles by code.</summary>
public interface IDefaultRateCycleStore
{
    DefaultRateCycle? Find(string code);
    void Save(DefaultRateCycle item);
    bool Remove(string code);
    List<DefaultRateCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultRateCycleStore(Dictionary<string, DefaultRateCycle> items, List<string> order) : IDefaultRateCycleStore
{
    public Dictionary<string, DefaultRateCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultRateCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultRateCycle item)
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

    public List<DefaultRateCycle> List()
    {
        var outValue = new List<DefaultRateCycle>();
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
public sealed class CachedDefaultRateCycleStore(MemoryDefaultRateCycleStore inner, DefaultRateCycleCache cache, int hits, int misses) : IDefaultRateCycleStore
{
    public MemoryDefaultRateCycleStore Inner { get; set; } = inner;
    public DefaultRateCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultRateCycle? Find(string code)
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

    public void Save(DefaultRateCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultRateCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default rate cycles. Empty fields match all.</summary>
public sealed record DefaultRateCycleQuery(string Region, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultRateCycle item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultRateCycle> Apply(List<DefaultRateCycle> items)
    {
        var matched = Collections.Filter(items, (DefaultRateCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default rate cycle store.</summary>
public static class DefaultRateCycleStore
{
    public static IDefaultRateCycleStore NewMemoryDefaultRateCycleStore()
    {
        return new MemoryDefaultRateCycleStore(new Dictionary<string, DefaultRateCycle>(), new List<string>());
    }

    public static IDefaultRateCycleStore NewCachedDefaultRateCycleStore(MemoryDefaultRateCycleStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultRateCycleStore(inner, DefaultRateCycleCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<DefaultRateCycle> FindDefaultRateCycles(IDefaultRateCycleStore store, DefaultRateCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
