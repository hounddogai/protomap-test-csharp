namespace Acme.Workforce.Compensation.Adjustment.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary adjustment preferences by code.</summary>
public interface IPrimaryAdjustmentPreferenceStore
{
    PrimaryAdjustmentPreference? Find(string code);
    void Save(PrimaryAdjustmentPreference item);
    bool Remove(string code);
    List<PrimaryAdjustmentPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryAdjustmentPreferenceStore(Dictionary<string, PrimaryAdjustmentPreference> items, List<string> order) : IPrimaryAdjustmentPreferenceStore
{
    public Dictionary<string, PrimaryAdjustmentPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryAdjustmentPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryAdjustmentPreference item)
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

    public List<PrimaryAdjustmentPreference> List()
    {
        var outValue = new List<PrimaryAdjustmentPreference>();
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
public sealed class CachedPrimaryAdjustmentPreferenceStore(MemoryPrimaryAdjustmentPreferenceStore inner, PrimaryAdjustmentPreferenceCache cache, int hits, int misses) : IPrimaryAdjustmentPreferenceStore
{
    public MemoryPrimaryAdjustmentPreferenceStore Inner { get; set; } = inner;
    public PrimaryAdjustmentPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryAdjustmentPreference? Find(string code)
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

    public void Save(PrimaryAdjustmentPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryAdjustmentPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary adjustment preferences. Empty fields match all.</summary>
public sealed record PrimaryAdjustmentPreferenceQuery(string Label, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryAdjustmentPreference item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryAdjustmentPreference> Apply(List<PrimaryAdjustmentPreference> items)
    {
        var matched = Collections.Filter(items, (PrimaryAdjustmentPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary adjustment preference store.</summary>
public static class PrimaryAdjustmentPreferenceStore
{
    public static IPrimaryAdjustmentPreferenceStore NewMemoryPrimaryAdjustmentPreferenceStore()
    {
        return new MemoryPrimaryAdjustmentPreferenceStore(
            new Dictionary<string, PrimaryAdjustmentPreference>(),
            new List<string>());
    }

    public static IPrimaryAdjustmentPreferenceStore NewCachedPrimaryAdjustmentPreferenceStore(MemoryPrimaryAdjustmentPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryAdjustmentPreferenceStore(
            inner,
            PrimaryAdjustmentPreferenceCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryAdjustmentPreference> FindPrimaryAdjustmentPreferences(IPrimaryAdjustmentPreferenceStore store, PrimaryAdjustmentPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
