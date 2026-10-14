namespace Acme.Workforce.Assets.Refresh.RefreshRequest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global refresh overrides by code.</summary>
public interface IGlobalRefreshOverrideStore
{
    GlobalRefreshOverride? Find(string code);
    void Save(GlobalRefreshOverride item);
    bool Remove(string code);
    List<GlobalRefreshOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalRefreshOverrideStore(Dictionary<string, GlobalRefreshOverride> items, List<string> order) : IGlobalRefreshOverrideStore
{
    public Dictionary<string, GlobalRefreshOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalRefreshOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalRefreshOverride item)
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

    public List<GlobalRefreshOverride> List()
    {
        var outValue = new List<GlobalRefreshOverride>();
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
public sealed class CachedGlobalRefreshOverrideStore(MemoryGlobalRefreshOverrideStore inner, GlobalRefreshOverrideCache cache, int hits, int misses) : IGlobalRefreshOverrideStore
{
    public MemoryGlobalRefreshOverrideStore Inner { get; set; } = inner;
    public GlobalRefreshOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalRefreshOverride? Find(string code)
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

    public void Save(GlobalRefreshOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalRefreshOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global refresh overrides. Empty fields match all.</summary>
public sealed record GlobalRefreshOverrideQuery(string Source, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalRefreshOverride item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalRefreshOverride> Apply(List<GlobalRefreshOverride> items)
    {
        var matched = Collections.Filter(items, (GlobalRefreshOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global refresh override store.</summary>
public static class GlobalRefreshOverrideStore
{
    public static IGlobalRefreshOverrideStore NewMemoryGlobalRefreshOverrideStore()
    {
        return new MemoryGlobalRefreshOverrideStore(
            new Dictionary<string, GlobalRefreshOverride>(),
            new List<string>());
    }

    public static IGlobalRefreshOverrideStore NewCachedGlobalRefreshOverrideStore(MemoryGlobalRefreshOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalRefreshOverrideStore(inner, GlobalRefreshOverrideCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<GlobalRefreshOverride> FindGlobalRefreshOverrides(IGlobalRefreshOverrideStore store, GlobalRefreshOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
