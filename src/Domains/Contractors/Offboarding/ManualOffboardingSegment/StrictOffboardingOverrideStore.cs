namespace Acme.Workforce.Contractors.Offboarding.ManualOffboardingSegment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict offboarding overrides by code.</summary>
public interface IStrictOffboardingOverrideStore
{
    StrictOffboardingOverride? Find(string code);
    void Save(StrictOffboardingOverride item);
    bool Remove(string code);
    List<StrictOffboardingOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictOffboardingOverrideStore(Dictionary<string, StrictOffboardingOverride> items, List<string> order) : IStrictOffboardingOverrideStore
{
    public Dictionary<string, StrictOffboardingOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictOffboardingOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictOffboardingOverride item)
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

    public List<StrictOffboardingOverride> List()
    {
        var outValue = new List<StrictOffboardingOverride>();
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
public sealed class CachedStrictOffboardingOverrideStore(MemoryStrictOffboardingOverrideStore inner, StrictOffboardingOverrideCache cache, int hits, int misses) : IStrictOffboardingOverrideStore
{
    public MemoryStrictOffboardingOverrideStore Inner { get; set; } = inner;
    public StrictOffboardingOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictOffboardingOverride? Find(string code)
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

    public void Save(StrictOffboardingOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictOffboardingOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict offboarding overrides. Empty fields match all.</summary>
public sealed record StrictOffboardingOverrideQuery(string Title, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(StrictOffboardingOverride item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictOffboardingOverride> Apply(List<StrictOffboardingOverride> items)
    {
        var matched = Collections.Filter(items, (StrictOffboardingOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict offboarding override store.</summary>
public static class StrictOffboardingOverrideStore
{
    public static IStrictOffboardingOverrideStore NewMemoryStrictOffboardingOverrideStore()
    {
        return new MemoryStrictOffboardingOverrideStore(
            new Dictionary<string, StrictOffboardingOverride>(),
            new List<string>());
    }

    public static IStrictOffboardingOverrideStore NewCachedStrictOffboardingOverrideStore(MemoryStrictOffboardingOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedStrictOffboardingOverrideStore(
            inner,
            StrictOffboardingOverrideCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<StrictOffboardingOverride> FindStrictOffboardingOverrides(IStrictOffboardingOverrideStore store, StrictOffboardingOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
