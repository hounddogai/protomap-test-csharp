namespace Acme.Workforce.Workplace.AccessBadge.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps access badge adjustments by code.</summary>
public interface IAccessBadgeAdjustmentStore
{
    AccessBadgeAdjustment? Find(string code);
    void Save(AccessBadgeAdjustment item);
    bool Remove(string code);
    List<AccessBadgeAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccessBadgeAdjustmentStore(Dictionary<string, AccessBadgeAdjustment> items, List<string> order) : IAccessBadgeAdjustmentStore
{
    public Dictionary<string, AccessBadgeAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccessBadgeAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccessBadgeAdjustment item)
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

    public List<AccessBadgeAdjustment> List()
    {
        var outValue = new List<AccessBadgeAdjustment>();
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
public sealed class CachedAccessBadgeAdjustmentStore(MemoryAccessBadgeAdjustmentStore inner, AccessBadgeAdjustmentCache cache, int hits, int misses) : IAccessBadgeAdjustmentStore
{
    public MemoryAccessBadgeAdjustmentStore Inner { get; set; } = inner;
    public AccessBadgeAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccessBadgeAdjustment? Find(string code)
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

    public void Save(AccessBadgeAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccessBadgeAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects access badge adjustments. Empty fields match all.</summary>
public sealed record AccessBadgeAdjustmentQuery(string Country, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(AccessBadgeAdjustment item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AccessBadgeAdjustment> Apply(List<AccessBadgeAdjustment> items)
    {
        var matched = Collections.Filter(items, (AccessBadgeAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of access badge adjustment store.</summary>
public static class AccessBadgeAdjustmentStore
{
    public static IAccessBadgeAdjustmentStore NewMemoryAccessBadgeAdjustmentStore()
    {
        return new MemoryAccessBadgeAdjustmentStore(
            new Dictionary<string, AccessBadgeAdjustment>(),
            new List<string>());
    }

    public static IAccessBadgeAdjustmentStore NewCachedAccessBadgeAdjustmentStore(MemoryAccessBadgeAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedAccessBadgeAdjustmentStore(inner, AccessBadgeAdjustmentCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<AccessBadgeAdjustment> FindAccessBadgeAdjustments(IAccessBadgeAdjustmentStore store, AccessBadgeAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
