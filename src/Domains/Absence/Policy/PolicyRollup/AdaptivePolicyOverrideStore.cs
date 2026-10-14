namespace Acme.Workforce.Absence.Policy.PolicyRollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive policy overrides by code.</summary>
public interface IAdaptivePolicyOverrideStore
{
    AdaptivePolicyOverride? Find(string code);
    void Save(AdaptivePolicyOverride item);
    bool Remove(string code);
    List<AdaptivePolicyOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptivePolicyOverrideStore(Dictionary<string, AdaptivePolicyOverride> items, List<string> order) : IAdaptivePolicyOverrideStore
{
    public Dictionary<string, AdaptivePolicyOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptivePolicyOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptivePolicyOverride item)
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

    public List<AdaptivePolicyOverride> List()
    {
        var outValue = new List<AdaptivePolicyOverride>();
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
public sealed class CachedAdaptivePolicyOverrideStore(MemoryAdaptivePolicyOverrideStore inner, AdaptivePolicyOverrideCache cache, int hits, int misses) : IAdaptivePolicyOverrideStore
{
    public MemoryAdaptivePolicyOverrideStore Inner { get; set; } = inner;
    public AdaptivePolicyOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptivePolicyOverride? Find(string code)
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

    public void Save(AdaptivePolicyOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptivePolicyOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive policy overrides. Empty fields match all.</summary>
public sealed record AdaptivePolicyOverrideQuery(string Team, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptivePolicyOverride item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptivePolicyOverride> Apply(List<AdaptivePolicyOverride> items)
    {
        var matched = Collections.Filter(items, (AdaptivePolicyOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive policy override store.</summary>
public static class AdaptivePolicyOverrideStore
{
    public static IAdaptivePolicyOverrideStore NewMemoryAdaptivePolicyOverrideStore()
    {
        return new MemoryAdaptivePolicyOverrideStore(
            new Dictionary<string, AdaptivePolicyOverride>(),
            new List<string>());
    }

    public static IAdaptivePolicyOverrideStore NewCachedAdaptivePolicyOverrideStore(MemoryAdaptivePolicyOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptivePolicyOverrideStore(
            inner,
            AdaptivePolicyOverrideCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<AdaptivePolicyOverride> FindAdaptivePolicyOverrides(IAdaptivePolicyOverrideStore store, AdaptivePolicyOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
