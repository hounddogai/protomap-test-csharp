namespace Acme.Workforce.Talent.Mobility.MobilityAudit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy mobility overrides by code.</summary>
public interface ILegacyMobilityOverrideStore
{
    LegacyMobilityOverride? Find(string code);
    void Save(LegacyMobilityOverride item);
    bool Remove(string code);
    List<LegacyMobilityOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyMobilityOverrideStore(Dictionary<string, LegacyMobilityOverride> items, List<string> order) : ILegacyMobilityOverrideStore
{
    public Dictionary<string, LegacyMobilityOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyMobilityOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyMobilityOverride item)
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

    public List<LegacyMobilityOverride> List()
    {
        var outValue = new List<LegacyMobilityOverride>();
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
public sealed class CachedLegacyMobilityOverrideStore(MemoryLegacyMobilityOverrideStore inner, LegacyMobilityOverrideCache cache, int hits, int misses) : ILegacyMobilityOverrideStore
{
    public MemoryLegacyMobilityOverrideStore Inner { get; set; } = inner;
    public LegacyMobilityOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyMobilityOverride? Find(string code)
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

    public void Save(LegacyMobilityOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyMobilityOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy mobility overrides. Empty fields match all.</summary>
public sealed record LegacyMobilityOverrideQuery(string Label, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyMobilityOverride item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyMobilityOverride> Apply(List<LegacyMobilityOverride> items)
    {
        var matched = Collections.Filter(items, (LegacyMobilityOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy mobility override store.</summary>
public static class LegacyMobilityOverrideStore
{
    public static ILegacyMobilityOverrideStore NewMemoryLegacyMobilityOverrideStore()
    {
        return new MemoryLegacyMobilityOverrideStore(
            new Dictionary<string, LegacyMobilityOverride>(),
            new List<string>());
    }

    public static ILegacyMobilityOverrideStore NewCachedLegacyMobilityOverrideStore(MemoryLegacyMobilityOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyMobilityOverrideStore(
            inner,
            LegacyMobilityOverrideCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<LegacyMobilityOverride> FindLegacyMobilityOverrides(ILegacyMobilityOverrideStore store, LegacyMobilityOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
