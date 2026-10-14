namespace Acme.Workforce.Absence.Policy.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling policy estimates by code.</summary>
public interface IRollingPolicyEstimateStore
{
    RollingPolicyEstimate? Find(string code);
    void Save(RollingPolicyEstimate item);
    bool Remove(string code);
    List<RollingPolicyEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingPolicyEstimateStore(Dictionary<string, RollingPolicyEstimate> items, List<string> order) : IRollingPolicyEstimateStore
{
    public Dictionary<string, RollingPolicyEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingPolicyEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingPolicyEstimate item)
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

    public List<RollingPolicyEstimate> List()
    {
        var outValue = new List<RollingPolicyEstimate>();
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
public sealed class CachedRollingPolicyEstimateStore(MemoryRollingPolicyEstimateStore inner, RollingPolicyEstimateCache cache, int hits, int misses) : IRollingPolicyEstimateStore
{
    public MemoryRollingPolicyEstimateStore Inner { get; set; } = inner;
    public RollingPolicyEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingPolicyEstimate? Find(string code)
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

    public void Save(RollingPolicyEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingPolicyEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling policy estimates. Empty fields match all.</summary>
public sealed record RollingPolicyEstimateQuery(string Title, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(RollingPolicyEstimate item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingPolicyEstimate> Apply(List<RollingPolicyEstimate> items)
    {
        var matched = Collections.Filter(items, (RollingPolicyEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling policy estimate store.</summary>
public static class RollingPolicyEstimateStore
{
    public static IRollingPolicyEstimateStore NewMemoryRollingPolicyEstimateStore()
    {
        return new MemoryRollingPolicyEstimateStore(
            new Dictionary<string, RollingPolicyEstimate>(),
            new List<string>());
    }

    public static IRollingPolicyEstimateStore NewCachedRollingPolicyEstimateStore(MemoryRollingPolicyEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedRollingPolicyEstimateStore(inner, RollingPolicyEstimateCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<RollingPolicyEstimate> FindRollingPolicyEstimates(IRollingPolicyEstimateStore store, RollingPolicyEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
