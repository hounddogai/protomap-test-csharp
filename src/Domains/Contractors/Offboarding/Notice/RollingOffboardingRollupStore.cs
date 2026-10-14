namespace Acme.Workforce.Contractors.Offboarding.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling offboarding rollups by code.</summary>
public interface IRollingOffboardingRollupStore
{
    RollingOffboardingRollup? Find(string code);
    void Save(RollingOffboardingRollup item);
    bool Remove(string code);
    List<RollingOffboardingRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingOffboardingRollupStore(Dictionary<string, RollingOffboardingRollup> items, List<string> order) : IRollingOffboardingRollupStore
{
    public Dictionary<string, RollingOffboardingRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingOffboardingRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingOffboardingRollup item)
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

    public List<RollingOffboardingRollup> List()
    {
        var outValue = new List<RollingOffboardingRollup>();
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
public sealed class CachedRollingOffboardingRollupStore(MemoryRollingOffboardingRollupStore inner, RollingOffboardingRollupCache cache, int hits, int misses) : IRollingOffboardingRollupStore
{
    public MemoryRollingOffboardingRollupStore Inner { get; set; } = inner;
    public RollingOffboardingRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingOffboardingRollup? Find(string code)
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

    public void Save(RollingOffboardingRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingOffboardingRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling offboarding rollups. Empty fields match all.</summary>
public sealed record RollingOffboardingRollupQuery(string Label, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(RollingOffboardingRollup item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingOffboardingRollup> Apply(List<RollingOffboardingRollup> items)
    {
        var matched = Collections.Filter(items, (RollingOffboardingRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling offboarding rollup store.</summary>
public static class RollingOffboardingRollupStore
{
    public static IRollingOffboardingRollupStore NewMemoryRollingOffboardingRollupStore()
    {
        return new MemoryRollingOffboardingRollupStore(
            new Dictionary<string, RollingOffboardingRollup>(),
            new List<string>());
    }

    public static IRollingOffboardingRollupStore NewCachedRollingOffboardingRollupStore(MemoryRollingOffboardingRollupStore inner, int ttl, IClock clock)
    {
        return new CachedRollingOffboardingRollupStore(
            inner,
            RollingOffboardingRollupCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<RollingOffboardingRollup> FindRollingOffboardingRollups(IRollingOffboardingRollupStore store, RollingOffboardingRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
