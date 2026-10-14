namespace Acme.Workforce.Orgdesign.Reorg.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps reorg preferences by code.</summary>
public interface IReorgPreferenceStore
{
    ReorgPreference? Find(string code);
    void Save(ReorgPreference item);
    bool Remove(string code);
    List<ReorgPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReorgPreferenceStore(Dictionary<string, ReorgPreference> items, List<string> order) : IReorgPreferenceStore
{
    public Dictionary<string, ReorgPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReorgPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReorgPreference item)
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

    public List<ReorgPreference> List()
    {
        var outValue = new List<ReorgPreference>();
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
public sealed class CachedReorgPreferenceStore(MemoryReorgPreferenceStore inner, ReorgPreferenceCache cache, int hits, int misses) : IReorgPreferenceStore
{
    public MemoryReorgPreferenceStore Inner { get; set; } = inner;
    public ReorgPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReorgPreference? Find(string code)
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

    public void Save(ReorgPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReorgPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects reorg preferences. Empty fields match all.</summary>
public sealed record ReorgPreferenceQuery(string Source, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(ReorgPreference item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ReorgPreference> Apply(List<ReorgPreference> items)
    {
        var matched = Collections.Filter(items, (ReorgPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of reorg preference store.</summary>
public static class ReorgPreferenceStore
{
    public static IReorgPreferenceStore NewMemoryReorgPreferenceStore()
    {
        return new MemoryReorgPreferenceStore(new Dictionary<string, ReorgPreference>(), new List<string>());
    }

    public static IReorgPreferenceStore NewCachedReorgPreferenceStore(MemoryReorgPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedReorgPreferenceStore(inner, ReorgPreferenceCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<ReorgPreference> FindReorgPreferences(IReorgPreferenceStore store, ReorgPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
