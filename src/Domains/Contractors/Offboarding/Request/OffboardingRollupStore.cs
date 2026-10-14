namespace Acme.Workforce.Contractors.Offboarding.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps offboarding rollups by code.</summary>
public interface IOffboardingRollupStore
{
    OffboardingRollup? Find(string code);
    void Save(OffboardingRollup item);
    bool Remove(string code);
    List<OffboardingRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOffboardingRollupStore(Dictionary<string, OffboardingRollup> items, List<string> order) : IOffboardingRollupStore
{
    public Dictionary<string, OffboardingRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OffboardingRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OffboardingRollup item)
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

    public List<OffboardingRollup> List()
    {
        var outValue = new List<OffboardingRollup>();
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
public sealed class CachedOffboardingRollupStore(MemoryOffboardingRollupStore inner, OffboardingRollupCache cache, int hits, int misses) : IOffboardingRollupStore
{
    public MemoryOffboardingRollupStore Inner { get; set; } = inner;
    public OffboardingRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OffboardingRollup? Find(string code)
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

    public void Save(OffboardingRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OffboardingRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects offboarding rollups. Empty fields match all.</summary>
public sealed record OffboardingRollupQuery(string Region, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(OffboardingRollup item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<OffboardingRollup> Apply(List<OffboardingRollup> items)
    {
        var matched = Collections.Filter(items, (OffboardingRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of offboarding rollup store.</summary>
public static class OffboardingRollupStore
{
    public static IOffboardingRollupStore NewMemoryOffboardingRollupStore()
    {
        return new MemoryOffboardingRollupStore(new Dictionary<string, OffboardingRollup>(), new List<string>());
    }

    public static IOffboardingRollupStore NewCachedOffboardingRollupStore(MemoryOffboardingRollupStore inner, int ttl, IClock clock)
    {
        return new CachedOffboardingRollupStore(inner, OffboardingRollupCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<OffboardingRollup> FindOffboardingRollups(IOffboardingRollupStore store, OffboardingRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
