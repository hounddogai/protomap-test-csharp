namespace Acme.Workforce.Contractors.Engagement.EngagementProfile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps engagement overrides by code.</summary>
public interface IEngagementOverrideStore
{
    EngagementOverride? Find(string code);
    void Save(EngagementOverride item);
    bool Remove(string code);
    List<EngagementOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryEngagementOverrideStore(Dictionary<string, EngagementOverride> items, List<string> order) : IEngagementOverrideStore
{
    public Dictionary<string, EngagementOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public EngagementOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(EngagementOverride item)
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

    public List<EngagementOverride> List()
    {
        var outValue = new List<EngagementOverride>();
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
public sealed class CachedEngagementOverrideStore(MemoryEngagementOverrideStore inner, EngagementOverrideCache cache, int hits, int misses) : IEngagementOverrideStore
{
    public MemoryEngagementOverrideStore Inner { get; set; } = inner;
    public EngagementOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public EngagementOverride? Find(string code)
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

    public void Save(EngagementOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<EngagementOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects engagement overrides. Empty fields match all.</summary>
public sealed record EngagementOverrideQuery(string Source, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(EngagementOverride item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<EngagementOverride> Apply(List<EngagementOverride> items)
    {
        var matched = Collections.Filter(items, (EngagementOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of engagement override store.</summary>
public static class EngagementOverrideStore
{
    public static IEngagementOverrideStore NewMemoryEngagementOverrideStore()
    {
        return new MemoryEngagementOverrideStore(new Dictionary<string, EngagementOverride>(), new List<string>());
    }

    public static IEngagementOverrideStore NewCachedEngagementOverrideStore(MemoryEngagementOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedEngagementOverrideStore(inner, EngagementOverrideCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<EngagementOverride> FindEngagementOverrides(IEngagementOverrideStore store, EngagementOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
