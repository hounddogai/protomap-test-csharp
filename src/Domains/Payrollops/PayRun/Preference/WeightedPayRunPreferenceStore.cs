namespace Acme.Workforce.Payrollops.PayRun.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted pay run preferences by code.</summary>
public interface IWeightedPayRunPreferenceStore
{
    WeightedPayRunPreference? Find(string code);
    void Save(WeightedPayRunPreference item);
    bool Remove(string code);
    List<WeightedPayRunPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedPayRunPreferenceStore(Dictionary<string, WeightedPayRunPreference> items, List<string> order) : IWeightedPayRunPreferenceStore
{
    public Dictionary<string, WeightedPayRunPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedPayRunPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedPayRunPreference item)
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

    public List<WeightedPayRunPreference> List()
    {
        var outValue = new List<WeightedPayRunPreference>();
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
public sealed class CachedWeightedPayRunPreferenceStore(MemoryWeightedPayRunPreferenceStore inner, WeightedPayRunPreferenceCache cache, int hits, int misses) : IWeightedPayRunPreferenceStore
{
    public MemoryWeightedPayRunPreferenceStore Inner { get; set; } = inner;
    public WeightedPayRunPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedPayRunPreference? Find(string code)
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

    public void Save(WeightedPayRunPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedPayRunPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted pay run preferences. Empty fields match all.</summary>
public sealed record WeightedPayRunPreferenceQuery(string Source, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedPayRunPreference item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedPayRunPreference> Apply(List<WeightedPayRunPreference> items)
    {
        var matched = Collections.Filter(items, (WeightedPayRunPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted pay run preference store.</summary>
public static class WeightedPayRunPreferenceStore
{
    public static IWeightedPayRunPreferenceStore NewMemoryWeightedPayRunPreferenceStore()
    {
        return new MemoryWeightedPayRunPreferenceStore(
            new Dictionary<string, WeightedPayRunPreference>(),
            new List<string>());
    }

    public static IWeightedPayRunPreferenceStore NewCachedWeightedPayRunPreferenceStore(MemoryWeightedPayRunPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedPayRunPreferenceStore(
            inner,
            WeightedPayRunPreferenceCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<WeightedPayRunPreference> FindWeightedPayRunPreferences(IWeightedPayRunPreferenceStore store, WeightedPayRunPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
