namespace Acme.Workforce.Contractors.Offboarding.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy offboarding rollups by code.</summary>
public interface ILegacyOffboardingRollupStore
{
    LegacyOffboardingRollup? Find(string code);
    void Save(LegacyOffboardingRollup item);
    bool Remove(string code);
    List<LegacyOffboardingRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyOffboardingRollupStore(Dictionary<string, LegacyOffboardingRollup> items, List<string> order) : ILegacyOffboardingRollupStore
{
    public Dictionary<string, LegacyOffboardingRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyOffboardingRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyOffboardingRollup item)
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

    public List<LegacyOffboardingRollup> List()
    {
        var outValue = new List<LegacyOffboardingRollup>();
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
public sealed class CachedLegacyOffboardingRollupStore(MemoryLegacyOffboardingRollupStore inner, LegacyOffboardingRollupCache cache, int hits, int misses) : ILegacyOffboardingRollupStore
{
    public MemoryLegacyOffboardingRollupStore Inner { get; set; } = inner;
    public LegacyOffboardingRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyOffboardingRollup? Find(string code)
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

    public void Save(LegacyOffboardingRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyOffboardingRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy offboarding rollups. Empty fields match all.</summary>
public sealed record LegacyOffboardingRollupQuery(string Region, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyOffboardingRollup item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyOffboardingRollup> Apply(List<LegacyOffboardingRollup> items)
    {
        var matched = Collections.Filter(items, (LegacyOffboardingRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy offboarding rollup store.</summary>
public static class LegacyOffboardingRollupStore
{
    public static ILegacyOffboardingRollupStore NewMemoryLegacyOffboardingRollupStore()
    {
        return new MemoryLegacyOffboardingRollupStore(
            new Dictionary<string, LegacyOffboardingRollup>(),
            new List<string>());
    }

    public static ILegacyOffboardingRollupStore NewCachedLegacyOffboardingRollupStore(MemoryLegacyOffboardingRollupStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyOffboardingRollupStore(
            inner,
            LegacyOffboardingRollupCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<LegacyOffboardingRollup> FindLegacyOffboardingRollups(ILegacyOffboardingRollupStore store, LegacyOffboardingRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
