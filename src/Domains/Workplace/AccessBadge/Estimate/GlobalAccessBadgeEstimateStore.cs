namespace Acme.Workforce.Workplace.AccessBadge.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global access badge estimates by code.</summary>
public interface IGlobalAccessBadgeEstimateStore
{
    GlobalAccessBadgeEstimate? Find(string code);
    void Save(GlobalAccessBadgeEstimate item);
    bool Remove(string code);
    List<GlobalAccessBadgeEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalAccessBadgeEstimateStore(Dictionary<string, GlobalAccessBadgeEstimate> items, List<string> order) : IGlobalAccessBadgeEstimateStore
{
    public Dictionary<string, GlobalAccessBadgeEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalAccessBadgeEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalAccessBadgeEstimate item)
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

    public List<GlobalAccessBadgeEstimate> List()
    {
        var outValue = new List<GlobalAccessBadgeEstimate>();
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
public sealed class CachedGlobalAccessBadgeEstimateStore(MemoryGlobalAccessBadgeEstimateStore inner, GlobalAccessBadgeEstimateCache cache, int hits, int misses) : IGlobalAccessBadgeEstimateStore
{
    public MemoryGlobalAccessBadgeEstimateStore Inner { get; set; } = inner;
    public GlobalAccessBadgeEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalAccessBadgeEstimate? Find(string code)
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

    public void Save(GlobalAccessBadgeEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalAccessBadgeEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global access badge estimates. Empty fields match all.</summary>
public sealed record GlobalAccessBadgeEstimateQuery(string Name, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalAccessBadgeEstimate item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<GlobalAccessBadgeEstimate> Apply(List<GlobalAccessBadgeEstimate> items)
    {
        var matched = Collections.Filter(items, (GlobalAccessBadgeEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global access badge estimate store.</summary>
public static class GlobalAccessBadgeEstimateStore
{
    public static IGlobalAccessBadgeEstimateStore NewMemoryGlobalAccessBadgeEstimateStore()
    {
        return new MemoryGlobalAccessBadgeEstimateStore(
            new Dictionary<string, GlobalAccessBadgeEstimate>(),
            new List<string>());
    }

    public static IGlobalAccessBadgeEstimateStore NewCachedGlobalAccessBadgeEstimateStore(MemoryGlobalAccessBadgeEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalAccessBadgeEstimateStore(
            inner,
            GlobalAccessBadgeEstimateCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<GlobalAccessBadgeEstimate> FindGlobalAccessBadgeEstimates(IGlobalAccessBadgeEstimateStore store, GlobalAccessBadgeEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
