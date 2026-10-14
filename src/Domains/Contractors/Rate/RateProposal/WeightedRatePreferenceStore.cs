namespace Acme.Workforce.Contractors.Rate.RateProposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted rate preferences by code.</summary>
public interface IWeightedRatePreferenceStore
{
    WeightedRatePreference? Find(string code);
    void Save(WeightedRatePreference item);
    bool Remove(string code);
    List<WeightedRatePreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedRatePreferenceStore(Dictionary<string, WeightedRatePreference> items, List<string> order) : IWeightedRatePreferenceStore
{
    public Dictionary<string, WeightedRatePreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedRatePreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedRatePreference item)
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

    public List<WeightedRatePreference> List()
    {
        var outValue = new List<WeightedRatePreference>();
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
public sealed class CachedWeightedRatePreferenceStore(MemoryWeightedRatePreferenceStore inner, WeightedRatePreferenceCache cache, int hits, int misses) : IWeightedRatePreferenceStore
{
    public MemoryWeightedRatePreferenceStore Inner { get; set; } = inner;
    public WeightedRatePreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedRatePreference? Find(string code)
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

    public void Save(WeightedRatePreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedRatePreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted rate preferences. Empty fields match all.</summary>
public sealed record WeightedRatePreferenceQuery(string Title, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedRatePreference item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedRatePreference> Apply(List<WeightedRatePreference> items)
    {
        var matched = Collections.Filter(items, (WeightedRatePreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted rate preference store.</summary>
public static class WeightedRatePreferenceStore
{
    public static IWeightedRatePreferenceStore NewMemoryWeightedRatePreferenceStore()
    {
        return new MemoryWeightedRatePreferenceStore(
            new Dictionary<string, WeightedRatePreference>(),
            new List<string>());
    }

    public static IWeightedRatePreferenceStore NewCachedWeightedRatePreferenceStore(MemoryWeightedRatePreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedRatePreferenceStore(
            inner,
            WeightedRatePreferenceCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<WeightedRatePreference> FindWeightedRatePreferences(IWeightedRatePreferenceStore store, WeightedRatePreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
