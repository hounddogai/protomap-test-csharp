namespace Acme.Workforce.Workplace.AccessBadge.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps access badge plans by code.</summary>
public interface IAccessBadgePlanStore
{
    AccessBadgePlan? Find(string code);
    void Save(AccessBadgePlan item);
    bool Remove(string code);
    List<AccessBadgePlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccessBadgePlanStore(Dictionary<string, AccessBadgePlan> items, List<string> order) : IAccessBadgePlanStore
{
    public Dictionary<string, AccessBadgePlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccessBadgePlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccessBadgePlan item)
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

    public List<AccessBadgePlan> List()
    {
        var outValue = new List<AccessBadgePlan>();
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
public sealed class CachedAccessBadgePlanStore(MemoryAccessBadgePlanStore inner, AccessBadgePlanCache cache, int hits, int misses) : IAccessBadgePlanStore
{
    public MemoryAccessBadgePlanStore Inner { get; set; } = inner;
    public AccessBadgePlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccessBadgePlan? Find(string code)
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

    public void Save(AccessBadgePlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccessBadgePlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects access badge plans. Empty fields match all.</summary>
public sealed record AccessBadgePlanQuery(string Locale, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(AccessBadgePlan item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
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

    public List<AccessBadgePlan> Apply(List<AccessBadgePlan> items)
    {
        var matched = Collections.Filter(items, (AccessBadgePlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of access badge plan store.</summary>
public static class AccessBadgePlanStore
{
    public static IAccessBadgePlanStore NewMemoryAccessBadgePlanStore()
    {
        return new MemoryAccessBadgePlanStore(new Dictionary<string, AccessBadgePlan>(), new List<string>());
    }

    public static IAccessBadgePlanStore NewCachedAccessBadgePlanStore(MemoryAccessBadgePlanStore inner, int ttl, IClock clock)
    {
        return new CachedAccessBadgePlanStore(inner, AccessBadgePlanCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<AccessBadgePlan> FindAccessBadgePlans(IAccessBadgePlanStore store, AccessBadgePlanQuery query)
    {
        return query.Apply(store.List());
    }
}
