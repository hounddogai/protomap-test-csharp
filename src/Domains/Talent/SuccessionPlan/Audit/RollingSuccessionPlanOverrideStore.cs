namespace Acme.Workforce.Talent.SuccessionPlan.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling succession plan overrides by code.</summary>
public interface IRollingSuccessionPlanOverrideStore
{
    RollingSuccessionPlanOverride? Find(string code);
    void Save(RollingSuccessionPlanOverride item);
    bool Remove(string code);
    List<RollingSuccessionPlanOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingSuccessionPlanOverrideStore(Dictionary<string, RollingSuccessionPlanOverride> items, List<string> order) : IRollingSuccessionPlanOverrideStore
{
    public Dictionary<string, RollingSuccessionPlanOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingSuccessionPlanOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingSuccessionPlanOverride item)
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

    public List<RollingSuccessionPlanOverride> List()
    {
        var outValue = new List<RollingSuccessionPlanOverride>();
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
public sealed class CachedRollingSuccessionPlanOverrideStore(MemoryRollingSuccessionPlanOverrideStore inner, RollingSuccessionPlanOverrideCache cache, int hits, int misses) : IRollingSuccessionPlanOverrideStore
{
    public MemoryRollingSuccessionPlanOverrideStore Inner { get; set; } = inner;
    public RollingSuccessionPlanOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingSuccessionPlanOverride? Find(string code)
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

    public void Save(RollingSuccessionPlanOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingSuccessionPlanOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling succession plan overrides. Empty fields match all.</summary>
public sealed record RollingSuccessionPlanOverrideQuery(string Region, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(RollingSuccessionPlanOverride item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingSuccessionPlanOverride> Apply(List<RollingSuccessionPlanOverride> items)
    {
        var matched = Collections.Filter(items, (RollingSuccessionPlanOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling succession plan override store.</summary>
public static class RollingSuccessionPlanOverrideStore
{
    public static IRollingSuccessionPlanOverrideStore NewMemoryRollingSuccessionPlanOverrideStore()
    {
        return new MemoryRollingSuccessionPlanOverrideStore(
            new Dictionary<string, RollingSuccessionPlanOverride>(),
            new List<string>());
    }

    public static IRollingSuccessionPlanOverrideStore NewCachedRollingSuccessionPlanOverrideStore(MemoryRollingSuccessionPlanOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedRollingSuccessionPlanOverrideStore(
            inner,
            RollingSuccessionPlanOverrideCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<RollingSuccessionPlanOverride> FindRollingSuccessionPlanOverrides(IRollingSuccessionPlanOverrideStore store, RollingSuccessionPlanOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
