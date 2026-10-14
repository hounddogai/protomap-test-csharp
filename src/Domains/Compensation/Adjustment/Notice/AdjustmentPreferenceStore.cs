namespace Acme.Workforce.Compensation.Adjustment.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adjustment preferences by code.</summary>
public interface IAdjustmentPreferenceStore
{
    AdjustmentPreference? Find(string code);
    void Save(AdjustmentPreference item);
    bool Remove(string code);
    List<AdjustmentPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdjustmentPreferenceStore(Dictionary<string, AdjustmentPreference> items, List<string> order) : IAdjustmentPreferenceStore
{
    public Dictionary<string, AdjustmentPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdjustmentPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdjustmentPreference item)
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

    public List<AdjustmentPreference> List()
    {
        var outValue = new List<AdjustmentPreference>();
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
public sealed class CachedAdjustmentPreferenceStore(MemoryAdjustmentPreferenceStore inner, AdjustmentPreferenceCache cache, int hits, int misses) : IAdjustmentPreferenceStore
{
    public MemoryAdjustmentPreferenceStore Inner { get; set; } = inner;
    public AdjustmentPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdjustmentPreference? Find(string code)
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

    public void Save(AdjustmentPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdjustmentPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adjustment preferences. Empty fields match all.</summary>
public sealed record AdjustmentPreferenceQuery(string Label, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(AdjustmentPreference item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdjustmentPreference> Apply(List<AdjustmentPreference> items)
    {
        var matched = Collections.Filter(items, (AdjustmentPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adjustment preference store.</summary>
public static class AdjustmentPreferenceStore
{
    public static IAdjustmentPreferenceStore NewMemoryAdjustmentPreferenceStore()
    {
        return new MemoryAdjustmentPreferenceStore(
            new Dictionary<string, AdjustmentPreference>(),
            new List<string>());
    }

    public static IAdjustmentPreferenceStore NewCachedAdjustmentPreferenceStore(MemoryAdjustmentPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedAdjustmentPreferenceStore(inner, AdjustmentPreferenceCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<AdjustmentPreference> FindAdjustmentPreferences(IAdjustmentPreferenceStore store, AdjustmentPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
