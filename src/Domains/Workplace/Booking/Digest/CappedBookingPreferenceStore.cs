namespace Acme.Workforce.Workplace.Booking.Digest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped booking preferences by code.</summary>
public interface ICappedBookingPreferenceStore
{
    CappedBookingPreference? Find(string code);
    void Save(CappedBookingPreference item);
    bool Remove(string code);
    List<CappedBookingPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedBookingPreferenceStore(Dictionary<string, CappedBookingPreference> items, List<string> order) : ICappedBookingPreferenceStore
{
    public Dictionary<string, CappedBookingPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedBookingPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedBookingPreference item)
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

    public List<CappedBookingPreference> List()
    {
        var outValue = new List<CappedBookingPreference>();
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
public sealed class CachedCappedBookingPreferenceStore(MemoryCappedBookingPreferenceStore inner, CappedBookingPreferenceCache cache, int hits, int misses) : ICappedBookingPreferenceStore
{
    public MemoryCappedBookingPreferenceStore Inner { get; set; } = inner;
    public CappedBookingPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedBookingPreference? Find(string code)
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

    public void Save(CappedBookingPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedBookingPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped booking preferences. Empty fields match all.</summary>
public sealed record CappedBookingPreferenceQuery(string Unit, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(CappedBookingPreference item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedBookingPreference> Apply(List<CappedBookingPreference> items)
    {
        var matched = Collections.Filter(items, (CappedBookingPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped booking preference store.</summary>
public static class CappedBookingPreferenceStore
{
    public static ICappedBookingPreferenceStore NewMemoryCappedBookingPreferenceStore()
    {
        return new MemoryCappedBookingPreferenceStore(
            new Dictionary<string, CappedBookingPreference>(),
            new List<string>());
    }

    public static ICappedBookingPreferenceStore NewCachedCappedBookingPreferenceStore(MemoryCappedBookingPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedCappedBookingPreferenceStore(
            inner,
            CappedBookingPreferenceCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<CappedBookingPreference> FindCappedBookingPreferences(ICappedBookingPreferenceStore store, CappedBookingPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
