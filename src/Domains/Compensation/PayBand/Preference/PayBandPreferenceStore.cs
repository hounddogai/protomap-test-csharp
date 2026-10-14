namespace Acme.Workforce.Compensation.PayBand.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay band preferences by code.</summary>
public interface IPayBandPreferenceStore
{
    PayBandPreference? Find(string code);
    void Save(PayBandPreference item);
    bool Remove(string code);
    List<PayBandPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayBandPreferenceStore(Dictionary<string, PayBandPreference> items, List<string> order) : IPayBandPreferenceStore
{
    public Dictionary<string, PayBandPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayBandPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayBandPreference item)
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

    public List<PayBandPreference> List()
    {
        var outValue = new List<PayBandPreference>();
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
public sealed class CachedPayBandPreferenceStore(MemoryPayBandPreferenceStore inner, PayBandPreferenceCache cache, int hits, int misses) : IPayBandPreferenceStore
{
    public MemoryPayBandPreferenceStore Inner { get; set; } = inner;
    public PayBandPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayBandPreference? Find(string code)
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

    public void Save(PayBandPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayBandPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay band preferences. Empty fields match all.</summary>
public sealed record PayBandPreferenceQuery(string Title, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(PayBandPreference item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayBandPreference> Apply(List<PayBandPreference> items)
    {
        var matched = Collections.Filter(items, (PayBandPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay band preference store.</summary>
public static class PayBandPreferenceStore
{
    public static IPayBandPreferenceStore NewMemoryPayBandPreferenceStore()
    {
        return new MemoryPayBandPreferenceStore(new Dictionary<string, PayBandPreference>(), new List<string>());
    }

    public static IPayBandPreferenceStore NewCachedPayBandPreferenceStore(MemoryPayBandPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedPayBandPreferenceStore(inner, PayBandPreferenceCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PayBandPreference> FindPayBandPreferences(IPayBandPreferenceStore store, PayBandPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
