namespace Acme.Workforce.Assets.Refresh.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps refresh overrides by code.</summary>
public interface IRefreshOverrideStore
{
    RefreshOverride? Find(string code);
    void Save(RefreshOverride item);
    bool Remove(string code);
    List<RefreshOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRefreshOverrideStore(Dictionary<string, RefreshOverride> items, List<string> order) : IRefreshOverrideStore
{
    public Dictionary<string, RefreshOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RefreshOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RefreshOverride item)
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

    public List<RefreshOverride> List()
    {
        var outValue = new List<RefreshOverride>();
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
public sealed class CachedRefreshOverrideStore(MemoryRefreshOverrideStore inner, RefreshOverrideCache cache, int hits, int misses) : IRefreshOverrideStore
{
    public MemoryRefreshOverrideStore Inner { get; set; } = inner;
    public RefreshOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RefreshOverride? Find(string code)
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

    public void Save(RefreshOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RefreshOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects refresh overrides. Empty fields match all.</summary>
public sealed record RefreshOverrideQuery(string Label, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(RefreshOverride item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RefreshOverride> Apply(List<RefreshOverride> items)
    {
        var matched = Collections.Filter(items, (RefreshOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of refresh override store.</summary>
public static class RefreshOverrideStore
{
    public static IRefreshOverrideStore NewMemoryRefreshOverrideStore()
    {
        return new MemoryRefreshOverrideStore(new Dictionary<string, RefreshOverride>(), new List<string>());
    }

    public static IRefreshOverrideStore NewCachedRefreshOverrideStore(MemoryRefreshOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedRefreshOverrideStore(inner, RefreshOverrideCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<RefreshOverride> FindRefreshOverrides(IRefreshOverrideStore store, RefreshOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
