namespace Acme.Workforce.Absence.ReturnPlan.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient return plan overrides by code.</summary>
public interface ILenientReturnPlanOverrideStore
{
    LenientReturnPlanOverride? Find(string code);
    void Save(LenientReturnPlanOverride item);
    bool Remove(string code);
    List<LenientReturnPlanOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientReturnPlanOverrideStore(Dictionary<string, LenientReturnPlanOverride> items, List<string> order) : ILenientReturnPlanOverrideStore
{
    public Dictionary<string, LenientReturnPlanOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientReturnPlanOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientReturnPlanOverride item)
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

    public List<LenientReturnPlanOverride> List()
    {
        var outValue = new List<LenientReturnPlanOverride>();
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
public sealed class CachedLenientReturnPlanOverrideStore(MemoryLenientReturnPlanOverrideStore inner, LenientReturnPlanOverrideCache cache, int hits, int misses) : ILenientReturnPlanOverrideStore
{
    public MemoryLenientReturnPlanOverrideStore Inner { get; set; } = inner;
    public LenientReturnPlanOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientReturnPlanOverride? Find(string code)
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

    public void Save(LenientReturnPlanOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientReturnPlanOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient return plan overrides. Empty fields match all.</summary>
public sealed record LenientReturnPlanOverrideQuery(string Source, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(LenientReturnPlanOverride item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientReturnPlanOverride> Apply(List<LenientReturnPlanOverride> items)
    {
        var matched = Collections.Filter(items, (LenientReturnPlanOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient return plan override store.</summary>
public static class LenientReturnPlanOverrideStore
{
    public static ILenientReturnPlanOverrideStore NewMemoryLenientReturnPlanOverrideStore()
    {
        return new MemoryLenientReturnPlanOverrideStore(
            new Dictionary<string, LenientReturnPlanOverride>(),
            new List<string>());
    }

    public static ILenientReturnPlanOverrideStore NewCachedLenientReturnPlanOverrideStore(MemoryLenientReturnPlanOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedLenientReturnPlanOverrideStore(
            inner,
            LenientReturnPlanOverrideCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<LenientReturnPlanOverride> FindLenientReturnPlanOverrides(ILenientReturnPlanOverrideStore store, LenientReturnPlanOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
