namespace Acme.Workforce.Absence.ReturnPlan.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps return plan overrides by code.</summary>
public interface IReturnPlanOverrideStore
{
    ReturnPlanOverride? Find(string code);
    void Save(ReturnPlanOverride item);
    bool Remove(string code);
    List<ReturnPlanOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReturnPlanOverrideStore(Dictionary<string, ReturnPlanOverride> items, List<string> order) : IReturnPlanOverrideStore
{
    public Dictionary<string, ReturnPlanOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReturnPlanOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReturnPlanOverride item)
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

    public List<ReturnPlanOverride> List()
    {
        var outValue = new List<ReturnPlanOverride>();
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
public sealed class CachedReturnPlanOverrideStore(MemoryReturnPlanOverrideStore inner, ReturnPlanOverrideCache cache, int hits, int misses) : IReturnPlanOverrideStore
{
    public MemoryReturnPlanOverrideStore Inner { get; set; } = inner;
    public ReturnPlanOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReturnPlanOverride? Find(string code)
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

    public void Save(ReturnPlanOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReturnPlanOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects return plan overrides. Empty fields match all.</summary>
public sealed record ReturnPlanOverrideQuery(string Locale, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(ReturnPlanOverride item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ReturnPlanOverride> Apply(List<ReturnPlanOverride> items)
    {
        var matched = Collections.Filter(items, (ReturnPlanOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of return plan override store.</summary>
public static class ReturnPlanOverrideStore
{
    public static IReturnPlanOverrideStore NewMemoryReturnPlanOverrideStore()
    {
        return new MemoryReturnPlanOverrideStore(new Dictionary<string, ReturnPlanOverride>(), new List<string>());
    }

    public static IReturnPlanOverrideStore NewCachedReturnPlanOverrideStore(MemoryReturnPlanOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedReturnPlanOverrideStore(inner, ReturnPlanOverrideCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<ReturnPlanOverride> FindReturnPlanOverrides(IReturnPlanOverrideStore store, ReturnPlanOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
