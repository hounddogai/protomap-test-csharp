namespace Acme.Workforce.Compensation.MeritCycle.CycleOverride;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual merit cycle overrides by code.</summary>
public interface IManualMeritCycleOverrideStore
{
    ManualMeritCycleOverride? Find(string code);
    void Save(ManualMeritCycleOverride item);
    bool Remove(string code);
    List<ManualMeritCycleOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualMeritCycleOverrideStore(Dictionary<string, ManualMeritCycleOverride> items, List<string> order) : IManualMeritCycleOverrideStore
{
    public Dictionary<string, ManualMeritCycleOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualMeritCycleOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualMeritCycleOverride item)
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

    public List<ManualMeritCycleOverride> List()
    {
        var outValue = new List<ManualMeritCycleOverride>();
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
public sealed class CachedManualMeritCycleOverrideStore(MemoryManualMeritCycleOverrideStore inner, ManualMeritCycleOverrideCache cache, int hits, int misses) : IManualMeritCycleOverrideStore
{
    public MemoryManualMeritCycleOverrideStore Inner { get; set; } = inner;
    public ManualMeritCycleOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualMeritCycleOverride? Find(string code)
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

    public void Save(ManualMeritCycleOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualMeritCycleOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual merit cycle overrides. Empty fields match all.</summary>
public sealed record ManualMeritCycleOverrideQuery(string Label, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(ManualMeritCycleOverride item)
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

    public List<ManualMeritCycleOverride> Apply(List<ManualMeritCycleOverride> items)
    {
        var matched = Collections.Filter(items, (ManualMeritCycleOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual merit cycle override store.</summary>
public static class ManualMeritCycleOverrideStore
{
    public static IManualMeritCycleOverrideStore NewMemoryManualMeritCycleOverrideStore()
    {
        return new MemoryManualMeritCycleOverrideStore(
            new Dictionary<string, ManualMeritCycleOverride>(),
            new List<string>());
    }

    public static IManualMeritCycleOverrideStore NewCachedManualMeritCycleOverrideStore(MemoryManualMeritCycleOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedManualMeritCycleOverrideStore(
            inner,
            ManualMeritCycleOverrideCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<ManualMeritCycleOverride> FindManualMeritCycleOverrides(IManualMeritCycleOverrideStore store, ManualMeritCycleOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
