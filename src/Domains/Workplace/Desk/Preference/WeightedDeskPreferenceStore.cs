namespace Acme.Workforce.Workplace.Desk.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted desk preferences by code.</summary>
public interface IWeightedDeskPreferenceStore
{
    WeightedDeskPreference? Find(string code);
    void Save(WeightedDeskPreference item);
    bool Remove(string code);
    List<WeightedDeskPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedDeskPreferenceStore(Dictionary<string, WeightedDeskPreference> items, List<string> order) : IWeightedDeskPreferenceStore
{
    public Dictionary<string, WeightedDeskPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedDeskPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedDeskPreference item)
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

    public List<WeightedDeskPreference> List()
    {
        var outValue = new List<WeightedDeskPreference>();
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
public sealed class CachedWeightedDeskPreferenceStore(MemoryWeightedDeskPreferenceStore inner, WeightedDeskPreferenceCache cache, int hits, int misses) : IWeightedDeskPreferenceStore
{
    public MemoryWeightedDeskPreferenceStore Inner { get; set; } = inner;
    public WeightedDeskPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedDeskPreference? Find(string code)
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

    public void Save(WeightedDeskPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedDeskPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted desk preferences. Empty fields match all.</summary>
public sealed record WeightedDeskPreferenceQuery(string Region, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedDeskPreference item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedDeskPreference> Apply(List<WeightedDeskPreference> items)
    {
        var matched = Collections.Filter(items, (WeightedDeskPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted desk preference store.</summary>
public static class WeightedDeskPreferenceStore
{
    public static IWeightedDeskPreferenceStore NewMemoryWeightedDeskPreferenceStore()
    {
        return new MemoryWeightedDeskPreferenceStore(
            new Dictionary<string, WeightedDeskPreference>(),
            new List<string>());
    }

    public static IWeightedDeskPreferenceStore NewCachedWeightedDeskPreferenceStore(MemoryWeightedDeskPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedDeskPreferenceStore(
            inner,
            WeightedDeskPreferenceCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<WeightedDeskPreference> FindWeightedDeskPreferences(IWeightedDeskPreferenceStore store, WeightedDeskPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
