namespace Acme.Workforce.Payrollops.Deduction.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy deduction preferences by code.</summary>
public interface ILegacyDeductionPreferenceStore
{
    LegacyDeductionPreference? Find(string code);
    void Save(LegacyDeductionPreference item);
    bool Remove(string code);
    List<LegacyDeductionPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyDeductionPreferenceStore(Dictionary<string, LegacyDeductionPreference> items, List<string> order) : ILegacyDeductionPreferenceStore
{
    public Dictionary<string, LegacyDeductionPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyDeductionPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyDeductionPreference item)
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

    public List<LegacyDeductionPreference> List()
    {
        var outValue = new List<LegacyDeductionPreference>();
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
public sealed class CachedLegacyDeductionPreferenceStore(MemoryLegacyDeductionPreferenceStore inner, LegacyDeductionPreferenceCache cache, int hits, int misses) : ILegacyDeductionPreferenceStore
{
    public MemoryLegacyDeductionPreferenceStore Inner { get; set; } = inner;
    public LegacyDeductionPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyDeductionPreference? Find(string code)
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

    public void Save(LegacyDeductionPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyDeductionPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy deduction preferences. Empty fields match all.</summary>
public sealed record LegacyDeductionPreferenceQuery(string Category, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyDeductionPreference item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<LegacyDeductionPreference> Apply(List<LegacyDeductionPreference> items)
    {
        var matched = Collections.Filter(items, (LegacyDeductionPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy deduction preference store.</summary>
public static class LegacyDeductionPreferenceStore
{
    public static ILegacyDeductionPreferenceStore NewMemoryLegacyDeductionPreferenceStore()
    {
        return new MemoryLegacyDeductionPreferenceStore(
            new Dictionary<string, LegacyDeductionPreference>(),
            new List<string>());
    }

    public static ILegacyDeductionPreferenceStore NewCachedLegacyDeductionPreferenceStore(MemoryLegacyDeductionPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyDeductionPreferenceStore(
            inner,
            LegacyDeductionPreferenceCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<LegacyDeductionPreference> FindLegacyDeductionPreferences(ILegacyDeductionPreferenceStore store, LegacyDeductionPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
