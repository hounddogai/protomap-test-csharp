namespace Acme.Workforce.Absence.Balance.BalanceGroup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps balance preferences by code.</summary>
public interface IBalancePreferenceStore
{
    BalancePreference? Find(string code);
    void Save(BalancePreference item);
    bool Remove(string code);
    List<BalancePreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBalancePreferenceStore(Dictionary<string, BalancePreference> items, List<string> order) : IBalancePreferenceStore
{
    public Dictionary<string, BalancePreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BalancePreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BalancePreference item)
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

    public List<BalancePreference> List()
    {
        var outValue = new List<BalancePreference>();
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
public sealed class CachedBalancePreferenceStore(MemoryBalancePreferenceStore inner, BalancePreferenceCache cache, int hits, int misses) : IBalancePreferenceStore
{
    public MemoryBalancePreferenceStore Inner { get; set; } = inner;
    public BalancePreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BalancePreference? Find(string code)
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

    public void Save(BalancePreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BalancePreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects balance preferences. Empty fields match all.</summary>
public sealed record BalancePreferenceQuery(string Unit, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(BalancePreference item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BalancePreference> Apply(List<BalancePreference> items)
    {
        var matched = Collections.Filter(items, (BalancePreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of balance preference store.</summary>
public static class BalancePreferenceStore
{
    public static IBalancePreferenceStore NewMemoryBalancePreferenceStore()
    {
        return new MemoryBalancePreferenceStore(new Dictionary<string, BalancePreference>(), new List<string>());
    }

    public static IBalancePreferenceStore NewCachedBalancePreferenceStore(MemoryBalancePreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedBalancePreferenceStore(inner, BalancePreferenceCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<BalancePreference> FindBalancePreferences(IBalancePreferenceStore store, BalancePreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
