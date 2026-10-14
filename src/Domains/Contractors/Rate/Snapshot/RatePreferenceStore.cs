namespace Acme.Workforce.Contractors.Rate.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rate preferences by code.</summary>
public interface IRatePreferenceStore
{
    RatePreference? Find(string code);
    void Save(RatePreference item);
    bool Remove(string code);
    List<RatePreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRatePreferenceStore(Dictionary<string, RatePreference> items, List<string> order) : IRatePreferenceStore
{
    public Dictionary<string, RatePreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RatePreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RatePreference item)
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

    public List<RatePreference> List()
    {
        var outValue = new List<RatePreference>();
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
public sealed class CachedRatePreferenceStore(MemoryRatePreferenceStore inner, RatePreferenceCache cache, int hits, int misses) : IRatePreferenceStore
{
    public MemoryRatePreferenceStore Inner { get; set; } = inner;
    public RatePreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RatePreference? Find(string code)
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

    public void Save(RatePreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RatePreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rate preferences. Empty fields match all.</summary>
public sealed record RatePreferenceQuery(string Source, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(RatePreference item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
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

    public List<RatePreference> Apply(List<RatePreference> items)
    {
        var matched = Collections.Filter(items, (RatePreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rate preference store.</summary>
public static class RatePreferenceStore
{
    public static IRatePreferenceStore NewMemoryRatePreferenceStore()
    {
        return new MemoryRatePreferenceStore(new Dictionary<string, RatePreference>(), new List<string>());
    }

    public static IRatePreferenceStore NewCachedRatePreferenceStore(MemoryRatePreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedRatePreferenceStore(inner, RatePreferenceCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<RatePreference> FindRatePreferences(IRatePreferenceStore store, RatePreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
