namespace Acme.Workforce.Orgdesign.Scenario.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy scenario overrides by code.</summary>
public interface ILegacyScenarioOverrideStore
{
    LegacyScenarioOverride? Find(string code);
    void Save(LegacyScenarioOverride item);
    bool Remove(string code);
    List<LegacyScenarioOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyScenarioOverrideStore(Dictionary<string, LegacyScenarioOverride> items, List<string> order) : ILegacyScenarioOverrideStore
{
    public Dictionary<string, LegacyScenarioOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyScenarioOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyScenarioOverride item)
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

    public List<LegacyScenarioOverride> List()
    {
        var outValue = new List<LegacyScenarioOverride>();
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
public sealed class CachedLegacyScenarioOverrideStore(MemoryLegacyScenarioOverrideStore inner, LegacyScenarioOverrideCache cache, int hits, int misses) : ILegacyScenarioOverrideStore
{
    public MemoryLegacyScenarioOverrideStore Inner { get; set; } = inner;
    public LegacyScenarioOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyScenarioOverride? Find(string code)
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

    public void Save(LegacyScenarioOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyScenarioOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy scenario overrides. Empty fields match all.</summary>
public sealed record LegacyScenarioOverrideQuery(string Title, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyScenarioOverride item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<LegacyScenarioOverride> Apply(List<LegacyScenarioOverride> items)
    {
        var matched = Collections.Filter(items, (LegacyScenarioOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy scenario override store.</summary>
public static class LegacyScenarioOverrideStore
{
    public static ILegacyScenarioOverrideStore NewMemoryLegacyScenarioOverrideStore()
    {
        return new MemoryLegacyScenarioOverrideStore(
            new Dictionary<string, LegacyScenarioOverride>(),
            new List<string>());
    }

    public static ILegacyScenarioOverrideStore NewCachedLegacyScenarioOverrideStore(MemoryLegacyScenarioOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyScenarioOverrideStore(
            inner,
            LegacyScenarioOverrideCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<LegacyScenarioOverride> FindLegacyScenarioOverrides(ILegacyScenarioOverrideStore store, LegacyScenarioOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
