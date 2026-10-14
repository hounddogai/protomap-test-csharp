namespace Acme.Workforce.Talent.SuccessionPlan.Milestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps succession plan overrides by code.</summary>
public interface ISuccessionPlanOverrideStore
{
    SuccessionPlanOverride? Find(string code);
    void Save(SuccessionPlanOverride item);
    bool Remove(string code);
    List<SuccessionPlanOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySuccessionPlanOverrideStore(Dictionary<string, SuccessionPlanOverride> items, List<string> order) : ISuccessionPlanOverrideStore
{
    public Dictionary<string, SuccessionPlanOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SuccessionPlanOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SuccessionPlanOverride item)
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

    public List<SuccessionPlanOverride> List()
    {
        var outValue = new List<SuccessionPlanOverride>();
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
public sealed class CachedSuccessionPlanOverrideStore(MemorySuccessionPlanOverrideStore inner, SuccessionPlanOverrideCache cache, int hits, int misses) : ISuccessionPlanOverrideStore
{
    public MemorySuccessionPlanOverrideStore Inner { get; set; } = inner;
    public SuccessionPlanOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SuccessionPlanOverride? Find(string code)
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

    public void Save(SuccessionPlanOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SuccessionPlanOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects succession plan overrides. Empty fields match all.</summary>
public sealed record SuccessionPlanOverrideQuery(string Locale, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(SuccessionPlanOverride item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SuccessionPlanOverride> Apply(List<SuccessionPlanOverride> items)
    {
        var matched = Collections.Filter(items, (SuccessionPlanOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of succession plan override store.</summary>
public static class SuccessionPlanOverrideStore
{
    public static ISuccessionPlanOverrideStore NewMemorySuccessionPlanOverrideStore()
    {
        return new MemorySuccessionPlanOverrideStore(
            new Dictionary<string, SuccessionPlanOverride>(),
            new List<string>());
    }

    public static ISuccessionPlanOverrideStore NewCachedSuccessionPlanOverrideStore(MemorySuccessionPlanOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedSuccessionPlanOverrideStore(
            inner,
            SuccessionPlanOverrideCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<SuccessionPlanOverride> FindSuccessionPlanOverrides(ISuccessionPlanOverrideStore store, SuccessionPlanOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
