namespace Acme.Workforce.Workplace.AccessBadge.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps access badge thresholds by code.</summary>
public interface IAccessBadgeThresholdStore
{
    AccessBadgeThreshold? Find(string code);
    void Save(AccessBadgeThreshold item);
    bool Remove(string code);
    List<AccessBadgeThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccessBadgeThresholdStore(Dictionary<string, AccessBadgeThreshold> items, List<string> order) : IAccessBadgeThresholdStore
{
    public Dictionary<string, AccessBadgeThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccessBadgeThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccessBadgeThreshold item)
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

    public List<AccessBadgeThreshold> List()
    {
        var outValue = new List<AccessBadgeThreshold>();
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
public sealed class CachedAccessBadgeThresholdStore(MemoryAccessBadgeThresholdStore inner, AccessBadgeThresholdCache cache, int hits, int misses) : IAccessBadgeThresholdStore
{
    public MemoryAccessBadgeThresholdStore Inner { get; set; } = inner;
    public AccessBadgeThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccessBadgeThreshold? Find(string code)
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

    public void Save(AccessBadgeThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccessBadgeThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects access badge thresholds. Empty fields match all.</summary>
public sealed record AccessBadgeThresholdQuery(string Unit, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(AccessBadgeThreshold item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
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

    public List<AccessBadgeThreshold> Apply(List<AccessBadgeThreshold> items)
    {
        var matched = Collections.Filter(items, (AccessBadgeThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of access badge threshold store.</summary>
public static class AccessBadgeThresholdStore
{
    public static IAccessBadgeThresholdStore NewMemoryAccessBadgeThresholdStore()
    {
        return new MemoryAccessBadgeThresholdStore(
            new Dictionary<string, AccessBadgeThreshold>(),
            new List<string>());
    }

    public static IAccessBadgeThresholdStore NewCachedAccessBadgeThresholdStore(MemoryAccessBadgeThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedAccessBadgeThresholdStore(inner, AccessBadgeThresholdCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<AccessBadgeThreshold> FindAccessBadgeThresholds(IAccessBadgeThresholdStore store, AccessBadgeThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
