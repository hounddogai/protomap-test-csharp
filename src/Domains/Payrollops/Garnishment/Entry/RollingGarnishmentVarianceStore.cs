namespace Acme.Workforce.Payrollops.Garnishment.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling garnishment variances by code.</summary>
public interface IRollingGarnishmentVarianceStore
{
    RollingGarnishmentVariance? Find(string code);
    void Save(RollingGarnishmentVariance item);
    bool Remove(string code);
    List<RollingGarnishmentVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingGarnishmentVarianceStore(Dictionary<string, RollingGarnishmentVariance> items, List<string> order) : IRollingGarnishmentVarianceStore
{
    public Dictionary<string, RollingGarnishmentVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingGarnishmentVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingGarnishmentVariance item)
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

    public List<RollingGarnishmentVariance> List()
    {
        var outValue = new List<RollingGarnishmentVariance>();
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
public sealed class CachedRollingGarnishmentVarianceStore(MemoryRollingGarnishmentVarianceStore inner, RollingGarnishmentVarianceCache cache, int hits, int misses) : IRollingGarnishmentVarianceStore
{
    public MemoryRollingGarnishmentVarianceStore Inner { get; set; } = inner;
    public RollingGarnishmentVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingGarnishmentVariance? Find(string code)
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

    public void Save(RollingGarnishmentVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingGarnishmentVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling garnishment variances. Empty fields match all.</summary>
public sealed record RollingGarnishmentVarianceQuery(string Category, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(RollingGarnishmentVariance item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingGarnishmentVariance> Apply(List<RollingGarnishmentVariance> items)
    {
        var matched = Collections.Filter(items, (RollingGarnishmentVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling garnishment variance store.</summary>
public static class RollingGarnishmentVarianceStore
{
    public static IRollingGarnishmentVarianceStore NewMemoryRollingGarnishmentVarianceStore()
    {
        return new MemoryRollingGarnishmentVarianceStore(
            new Dictionary<string, RollingGarnishmentVariance>(),
            new List<string>());
    }

    public static IRollingGarnishmentVarianceStore NewCachedRollingGarnishmentVarianceStore(MemoryRollingGarnishmentVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedRollingGarnishmentVarianceStore(
            inner,
            RollingGarnishmentVarianceCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<RollingGarnishmentVariance> FindRollingGarnishmentVariances(IRollingGarnishmentVarianceStore store, RollingGarnishmentVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
