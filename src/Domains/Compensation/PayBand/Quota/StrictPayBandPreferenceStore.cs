namespace Acme.Workforce.Compensation.PayBand.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict pay band preferences by code.</summary>
public interface IStrictPayBandPreferenceStore
{
    StrictPayBandPreference? Find(string code);
    void Save(StrictPayBandPreference item);
    bool Remove(string code);
    List<StrictPayBandPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictPayBandPreferenceStore(Dictionary<string, StrictPayBandPreference> items, List<string> order) : IStrictPayBandPreferenceStore
{
    public Dictionary<string, StrictPayBandPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictPayBandPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictPayBandPreference item)
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

    public List<StrictPayBandPreference> List()
    {
        var outValue = new List<StrictPayBandPreference>();
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
public sealed class CachedStrictPayBandPreferenceStore(MemoryStrictPayBandPreferenceStore inner, StrictPayBandPreferenceCache cache, int hits, int misses) : IStrictPayBandPreferenceStore
{
    public MemoryStrictPayBandPreferenceStore Inner { get; set; } = inner;
    public StrictPayBandPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictPayBandPreference? Find(string code)
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

    public void Save(StrictPayBandPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictPayBandPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict pay band preferences. Empty fields match all.</summary>
public sealed record StrictPayBandPreferenceQuery(string Name, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(StrictPayBandPreference item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictPayBandPreference> Apply(List<StrictPayBandPreference> items)
    {
        var matched = Collections.Filter(items, (StrictPayBandPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict pay band preference store.</summary>
public static class StrictPayBandPreferenceStore
{
    public static IStrictPayBandPreferenceStore NewMemoryStrictPayBandPreferenceStore()
    {
        return new MemoryStrictPayBandPreferenceStore(
            new Dictionary<string, StrictPayBandPreference>(),
            new List<string>());
    }

    public static IStrictPayBandPreferenceStore NewCachedStrictPayBandPreferenceStore(MemoryStrictPayBandPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedStrictPayBandPreferenceStore(
            inner,
            StrictPayBandPreferenceCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<StrictPayBandPreference> FindStrictPayBandPreferences(IStrictPayBandPreferenceStore store, StrictPayBandPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
