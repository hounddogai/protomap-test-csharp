namespace Acme.Workforce.Assets.Laptop.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling laptop baselines by code.</summary>
public interface IRollingLaptopBaselineStore
{
    RollingLaptopBaseline? Find(string code);
    void Save(RollingLaptopBaseline item);
    bool Remove(string code);
    List<RollingLaptopBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingLaptopBaselineStore(Dictionary<string, RollingLaptopBaseline> items, List<string> order) : IRollingLaptopBaselineStore
{
    public Dictionary<string, RollingLaptopBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingLaptopBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingLaptopBaseline item)
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

    public List<RollingLaptopBaseline> List()
    {
        var outValue = new List<RollingLaptopBaseline>();
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
public sealed class CachedRollingLaptopBaselineStore(MemoryRollingLaptopBaselineStore inner, RollingLaptopBaselineCache cache, int hits, int misses) : IRollingLaptopBaselineStore
{
    public MemoryRollingLaptopBaselineStore Inner { get; set; } = inner;
    public RollingLaptopBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingLaptopBaseline? Find(string code)
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

    public void Save(RollingLaptopBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingLaptopBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling laptop baselines. Empty fields match all.</summary>
public sealed record RollingLaptopBaselineQuery(string Region, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(RollingLaptopBaseline item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
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

    public List<RollingLaptopBaseline> Apply(List<RollingLaptopBaseline> items)
    {
        var matched = Collections.Filter(items, (RollingLaptopBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling laptop baseline store.</summary>
public static class RollingLaptopBaselineStore
{
    public static IRollingLaptopBaselineStore NewMemoryRollingLaptopBaselineStore()
    {
        return new MemoryRollingLaptopBaselineStore(
            new Dictionary<string, RollingLaptopBaseline>(),
            new List<string>());
    }

    public static IRollingLaptopBaselineStore NewCachedRollingLaptopBaselineStore(MemoryRollingLaptopBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedRollingLaptopBaselineStore(inner, RollingLaptopBaselineCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<RollingLaptopBaseline> FindRollingLaptopBaselines(IRollingLaptopBaselineStore store, RollingLaptopBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
