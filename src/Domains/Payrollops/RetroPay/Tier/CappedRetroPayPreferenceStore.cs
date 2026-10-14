namespace Acme.Workforce.Payrollops.RetroPay.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped retro pay preferences by code.</summary>
public interface ICappedRetroPayPreferenceStore
{
    CappedRetroPayPreference? Find(string code);
    void Save(CappedRetroPayPreference item);
    bool Remove(string code);
    List<CappedRetroPayPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedRetroPayPreferenceStore(Dictionary<string, CappedRetroPayPreference> items, List<string> order) : ICappedRetroPayPreferenceStore
{
    public Dictionary<string, CappedRetroPayPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedRetroPayPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedRetroPayPreference item)
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

    public List<CappedRetroPayPreference> List()
    {
        var outValue = new List<CappedRetroPayPreference>();
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
public sealed class CachedCappedRetroPayPreferenceStore(MemoryCappedRetroPayPreferenceStore inner, CappedRetroPayPreferenceCache cache, int hits, int misses) : ICappedRetroPayPreferenceStore
{
    public MemoryCappedRetroPayPreferenceStore Inner { get; set; } = inner;
    public CappedRetroPayPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedRetroPayPreference? Find(string code)
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

    public void Save(CappedRetroPayPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedRetroPayPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped retro pay preferences. Empty fields match all.</summary>
public sealed record CappedRetroPayPreferenceQuery(string Country, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(CappedRetroPayPreference item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
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

    public List<CappedRetroPayPreference> Apply(List<CappedRetroPayPreference> items)
    {
        var matched = Collections.Filter(items, (CappedRetroPayPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped retro pay preference store.</summary>
public static class CappedRetroPayPreferenceStore
{
    public static ICappedRetroPayPreferenceStore NewMemoryCappedRetroPayPreferenceStore()
    {
        return new MemoryCappedRetroPayPreferenceStore(
            new Dictionary<string, CappedRetroPayPreference>(),
            new List<string>());
    }

    public static ICappedRetroPayPreferenceStore NewCachedCappedRetroPayPreferenceStore(MemoryCappedRetroPayPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedCappedRetroPayPreferenceStore(
            inner,
            CappedRetroPayPreferenceCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<CappedRetroPayPreference> FindCappedRetroPayPreferences(ICappedRetroPayPreferenceStore store, CappedRetroPayPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
