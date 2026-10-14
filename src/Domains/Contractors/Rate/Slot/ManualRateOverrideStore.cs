namespace Acme.Workforce.Contractors.Rate.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual rate overrides by code.</summary>
public interface IManualRateOverrideStore
{
    ManualRateOverride? Find(string code);
    void Save(ManualRateOverride item);
    bool Remove(string code);
    List<ManualRateOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualRateOverrideStore(Dictionary<string, ManualRateOverride> items, List<string> order) : IManualRateOverrideStore
{
    public Dictionary<string, ManualRateOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualRateOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualRateOverride item)
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

    public List<ManualRateOverride> List()
    {
        var outValue = new List<ManualRateOverride>();
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
public sealed class CachedManualRateOverrideStore(MemoryManualRateOverrideStore inner, ManualRateOverrideCache cache, int hits, int misses) : IManualRateOverrideStore
{
    public MemoryManualRateOverrideStore Inner { get; set; } = inner;
    public ManualRateOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualRateOverride? Find(string code)
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

    public void Save(ManualRateOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualRateOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual rate overrides. Empty fields match all.</summary>
public sealed record ManualRateOverrideQuery(string Locale, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(ManualRateOverride item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualRateOverride> Apply(List<ManualRateOverride> items)
    {
        var matched = Collections.Filter(items, (ManualRateOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual rate override store.</summary>
public static class ManualRateOverrideStore
{
    public static IManualRateOverrideStore NewMemoryManualRateOverrideStore()
    {
        return new MemoryManualRateOverrideStore(new Dictionary<string, ManualRateOverride>(), new List<string>());
    }

    public static IManualRateOverrideStore NewCachedManualRateOverrideStore(MemoryManualRateOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedManualRateOverrideStore(inner, ManualRateOverrideCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<ManualRateOverride> FindManualRateOverrides(IManualRateOverrideStore store, ManualRateOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
