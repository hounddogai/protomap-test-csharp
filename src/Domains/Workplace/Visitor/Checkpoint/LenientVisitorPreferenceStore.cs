namespace Acme.Workforce.Workplace.Visitor.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient visitor preferences by code.</summary>
public interface ILenientVisitorPreferenceStore
{
    LenientVisitorPreference? Find(string code);
    void Save(LenientVisitorPreference item);
    bool Remove(string code);
    List<LenientVisitorPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientVisitorPreferenceStore(Dictionary<string, LenientVisitorPreference> items, List<string> order) : ILenientVisitorPreferenceStore
{
    public Dictionary<string, LenientVisitorPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientVisitorPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientVisitorPreference item)
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

    public List<LenientVisitorPreference> List()
    {
        var outValue = new List<LenientVisitorPreference>();
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
public sealed class CachedLenientVisitorPreferenceStore(MemoryLenientVisitorPreferenceStore inner, LenientVisitorPreferenceCache cache, int hits, int misses) : ILenientVisitorPreferenceStore
{
    public MemoryLenientVisitorPreferenceStore Inner { get; set; } = inner;
    public LenientVisitorPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientVisitorPreference? Find(string code)
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

    public void Save(LenientVisitorPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientVisitorPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient visitor preferences. Empty fields match all.</summary>
public sealed record LenientVisitorPreferenceQuery(string Category, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(LenientVisitorPreference item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<LenientVisitorPreference> Apply(List<LenientVisitorPreference> items)
    {
        var matched = Collections.Filter(items, (LenientVisitorPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient visitor preference store.</summary>
public static class LenientVisitorPreferenceStore
{
    public static ILenientVisitorPreferenceStore NewMemoryLenientVisitorPreferenceStore()
    {
        return new MemoryLenientVisitorPreferenceStore(
            new Dictionary<string, LenientVisitorPreference>(),
            new List<string>());
    }

    public static ILenientVisitorPreferenceStore NewCachedLenientVisitorPreferenceStore(MemoryLenientVisitorPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedLenientVisitorPreferenceStore(
            inner,
            LenientVisitorPreferenceCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<LenientVisitorPreference> FindLenientVisitorPreferences(ILenientVisitorPreferenceStore store, LenientVisitorPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
