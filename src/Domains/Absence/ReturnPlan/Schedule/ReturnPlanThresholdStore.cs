namespace Acme.Workforce.Absence.ReturnPlan.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps return plan thresholds by code.</summary>
public interface IReturnPlanThresholdStore
{
    ReturnPlanThreshold? Find(string code);
    void Save(ReturnPlanThreshold item);
    bool Remove(string code);
    List<ReturnPlanThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReturnPlanThresholdStore(Dictionary<string, ReturnPlanThreshold> items, List<string> order) : IReturnPlanThresholdStore
{
    public Dictionary<string, ReturnPlanThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReturnPlanThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReturnPlanThreshold item)
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

    public List<ReturnPlanThreshold> List()
    {
        var outValue = new List<ReturnPlanThreshold>();
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
public sealed class CachedReturnPlanThresholdStore(MemoryReturnPlanThresholdStore inner, ReturnPlanThresholdCache cache, int hits, int misses) : IReturnPlanThresholdStore
{
    public MemoryReturnPlanThresholdStore Inner { get; set; } = inner;
    public ReturnPlanThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReturnPlanThreshold? Find(string code)
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

    public void Save(ReturnPlanThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReturnPlanThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects return plan thresholds. Empty fields match all.</summary>
public sealed record ReturnPlanThresholdQuery(string Title, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(ReturnPlanThreshold item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<ReturnPlanThreshold> Apply(List<ReturnPlanThreshold> items)
    {
        var matched = Collections.Filter(items, (ReturnPlanThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of return plan threshold store.</summary>
public static class ReturnPlanThresholdStore
{
    public static IReturnPlanThresholdStore NewMemoryReturnPlanThresholdStore()
    {
        return new MemoryReturnPlanThresholdStore(new Dictionary<string, ReturnPlanThreshold>(), new List<string>());
    }

    public static IReturnPlanThresholdStore NewCachedReturnPlanThresholdStore(MemoryReturnPlanThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedReturnPlanThresholdStore(inner, ReturnPlanThresholdCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<ReturnPlanThreshold> FindReturnPlanThresholds(IReturnPlanThresholdStore store, ReturnPlanThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
