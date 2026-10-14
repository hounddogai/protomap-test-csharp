namespace Acme.Workforce.Absence.Accrual.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps accrual preferences by code.</summary>
public interface IAccrualPreferenceStore
{
    AccrualPreference? Find(string code);
    void Save(AccrualPreference item);
    bool Remove(string code);
    List<AccrualPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccrualPreferenceStore(Dictionary<string, AccrualPreference> items, List<string> order) : IAccrualPreferenceStore
{
    public Dictionary<string, AccrualPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccrualPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccrualPreference item)
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

    public List<AccrualPreference> List()
    {
        var outValue = new List<AccrualPreference>();
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
public sealed class CachedAccrualPreferenceStore(MemoryAccrualPreferenceStore inner, AccrualPreferenceCache cache, int hits, int misses) : IAccrualPreferenceStore
{
    public MemoryAccrualPreferenceStore Inner { get; set; } = inner;
    public AccrualPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccrualPreference? Find(string code)
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

    public void Save(AccrualPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccrualPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects accrual preferences. Empty fields match all.</summary>
public sealed record AccrualPreferenceQuery(string Department, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(AccrualPreference item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<AccrualPreference> Apply(List<AccrualPreference> items)
    {
        var matched = Collections.Filter(items, (AccrualPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of accrual preference store.</summary>
public static class AccrualPreferenceStore
{
    public static IAccrualPreferenceStore NewMemoryAccrualPreferenceStore()
    {
        return new MemoryAccrualPreferenceStore(new Dictionary<string, AccrualPreference>(), new List<string>());
    }

    public static IAccrualPreferenceStore NewCachedAccrualPreferenceStore(MemoryAccrualPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedAccrualPreferenceStore(inner, AccrualPreferenceCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<AccrualPreference> FindAccrualPreferences(IAccrualPreferenceStore store, AccrualPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
