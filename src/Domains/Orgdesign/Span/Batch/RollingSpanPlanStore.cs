namespace Acme.Workforce.Orgdesign.Span.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling span plans by code.</summary>
public interface IRollingSpanPlanStore
{
    RollingSpanPlan? Find(string code);
    void Save(RollingSpanPlan item);
    bool Remove(string code);
    List<RollingSpanPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingSpanPlanStore(Dictionary<string, RollingSpanPlan> items, List<string> order) : IRollingSpanPlanStore
{
    public Dictionary<string, RollingSpanPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingSpanPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingSpanPlan item)
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

    public List<RollingSpanPlan> List()
    {
        var outValue = new List<RollingSpanPlan>();
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
public sealed class CachedRollingSpanPlanStore(MemoryRollingSpanPlanStore inner, RollingSpanPlanCache cache, int hits, int misses) : IRollingSpanPlanStore
{
    public MemoryRollingSpanPlanStore Inner { get; set; } = inner;
    public RollingSpanPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingSpanPlan? Find(string code)
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

    public void Save(RollingSpanPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingSpanPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling span plans. Empty fields match all.</summary>
public sealed record RollingSpanPlanQuery(string Name, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(RollingSpanPlan item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<RollingSpanPlan> Apply(List<RollingSpanPlan> items)
    {
        var matched = Collections.Filter(items, (RollingSpanPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling span plan store.</summary>
public static class RollingSpanPlanStore
{
    public static IRollingSpanPlanStore NewMemoryRollingSpanPlanStore()
    {
        return new MemoryRollingSpanPlanStore(new Dictionary<string, RollingSpanPlan>(), new List<string>());
    }

    public static IRollingSpanPlanStore NewCachedRollingSpanPlanStore(MemoryRollingSpanPlanStore inner, int ttl, IClock clock)
    {
        return new CachedRollingSpanPlanStore(inner, RollingSpanPlanCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<RollingSpanPlan> FindRollingSpanPlans(IRollingSpanPlanStore store, RollingSpanPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
