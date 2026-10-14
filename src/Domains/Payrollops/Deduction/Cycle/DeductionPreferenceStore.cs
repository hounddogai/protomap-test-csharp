namespace Acme.Workforce.Payrollops.Deduction.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps deduction preferences by code.</summary>
public interface IDeductionPreferenceStore
{
    DeductionPreference? Find(string code);
    void Save(DeductionPreference item);
    bool Remove(string code);
    List<DeductionPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeductionPreferenceStore(Dictionary<string, DeductionPreference> items, List<string> order) : IDeductionPreferenceStore
{
    public Dictionary<string, DeductionPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeductionPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeductionPreference item)
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

    public List<DeductionPreference> List()
    {
        var outValue = new List<DeductionPreference>();
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
public sealed class CachedDeductionPreferenceStore(MemoryDeductionPreferenceStore inner, DeductionPreferenceCache cache, int hits, int misses) : IDeductionPreferenceStore
{
    public MemoryDeductionPreferenceStore Inner { get; set; } = inner;
    public DeductionPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeductionPreference? Find(string code)
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

    public void Save(DeductionPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeductionPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects deduction preferences. Empty fields match all.</summary>
public sealed record DeductionPreferenceQuery(string Name, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(DeductionPreference item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DeductionPreference> Apply(List<DeductionPreference> items)
    {
        var matched = Collections.Filter(items, (DeductionPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of deduction preference store.</summary>
public static class DeductionPreferenceStore
{
    public static IDeductionPreferenceStore NewMemoryDeductionPreferenceStore()
    {
        return new MemoryDeductionPreferenceStore(new Dictionary<string, DeductionPreference>(), new List<string>());
    }

    public static IDeductionPreferenceStore NewCachedDeductionPreferenceStore(MemoryDeductionPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedDeductionPreferenceStore(inner, DeductionPreferenceCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<DeductionPreference> FindDeductionPreferences(IDeductionPreferenceStore store, DeductionPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
