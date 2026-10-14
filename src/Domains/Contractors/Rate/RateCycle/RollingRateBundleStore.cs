namespace Acme.Workforce.Contractors.Rate.RateCycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling rate bundles by code.</summary>
public interface IRollingRateBundleStore
{
    RollingRateBundle? Find(string code);
    void Save(RollingRateBundle item);
    bool Remove(string code);
    List<RollingRateBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingRateBundleStore(Dictionary<string, RollingRateBundle> items, List<string> order) : IRollingRateBundleStore
{
    public Dictionary<string, RollingRateBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingRateBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingRateBundle item)
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

    public List<RollingRateBundle> List()
    {
        var outValue = new List<RollingRateBundle>();
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
public sealed class CachedRollingRateBundleStore(MemoryRollingRateBundleStore inner, RollingRateBundleCache cache, int hits, int misses) : IRollingRateBundleStore
{
    public MemoryRollingRateBundleStore Inner { get; set; } = inner;
    public RollingRateBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingRateBundle? Find(string code)
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

    public void Save(RollingRateBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingRateBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling rate bundles. Empty fields match all.</summary>
public sealed record RollingRateBundleQuery(string Channel, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(RollingRateBundle item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingRateBundle> Apply(List<RollingRateBundle> items)
    {
        var matched = Collections.Filter(items, (RollingRateBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling rate bundle store.</summary>
public static class RollingRateBundleStore
{
    public static IRollingRateBundleStore NewMemoryRollingRateBundleStore()
    {
        return new MemoryRollingRateBundleStore(new Dictionary<string, RollingRateBundle>(), new List<string>());
    }

    public static IRollingRateBundleStore NewCachedRollingRateBundleStore(MemoryRollingRateBundleStore inner, int ttl, IClock clock)
    {
        return new CachedRollingRateBundleStore(inner, RollingRateBundleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<RollingRateBundle> FindRollingRateBundles(IRollingRateBundleStore store, RollingRateBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
