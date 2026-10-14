namespace Acme.Workforce.Payrollops.PayRun.RunLedger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay run preferences by code.</summary>
public interface IPayRunPreferenceStore
{
    PayRunPreference? Find(string code);
    void Save(PayRunPreference item);
    bool Remove(string code);
    List<PayRunPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayRunPreferenceStore(Dictionary<string, PayRunPreference> items, List<string> order) : IPayRunPreferenceStore
{
    public Dictionary<string, PayRunPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayRunPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayRunPreference item)
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

    public List<PayRunPreference> List()
    {
        var outValue = new List<PayRunPreference>();
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
public sealed class CachedPayRunPreferenceStore(MemoryPayRunPreferenceStore inner, PayRunPreferenceCache cache, int hits, int misses) : IPayRunPreferenceStore
{
    public MemoryPayRunPreferenceStore Inner { get; set; } = inner;
    public PayRunPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayRunPreference? Find(string code)
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

    public void Save(PayRunPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayRunPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay run preferences. Empty fields match all.</summary>
public sealed record PayRunPreferenceQuery(string Country, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(PayRunPreference item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayRunPreference> Apply(List<PayRunPreference> items)
    {
        var matched = Collections.Filter(items, (PayRunPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay run preference store.</summary>
public static class PayRunPreferenceStore
{
    public static IPayRunPreferenceStore NewMemoryPayRunPreferenceStore()
    {
        return new MemoryPayRunPreferenceStore(new Dictionary<string, PayRunPreference>(), new List<string>());
    }

    public static IPayRunPreferenceStore NewCachedPayRunPreferenceStore(MemoryPayRunPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedPayRunPreferenceStore(inner, PayRunPreferenceCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PayRunPreference> FindPayRunPreferences(IPayRunPreferenceStore store, PayRunPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
