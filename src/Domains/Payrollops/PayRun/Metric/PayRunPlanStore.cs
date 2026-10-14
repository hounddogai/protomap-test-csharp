namespace Acme.Workforce.Payrollops.PayRun.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay run plans by code.</summary>
public interface IPayRunPlanStore
{
    PayRunPlan? Find(string code);
    void Save(PayRunPlan item);
    bool Remove(string code);
    List<PayRunPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayRunPlanStore(Dictionary<string, PayRunPlan> items, List<string> order) : IPayRunPlanStore
{
    public Dictionary<string, PayRunPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayRunPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayRunPlan item)
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

    public List<PayRunPlan> List()
    {
        var outValue = new List<PayRunPlan>();
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
public sealed class CachedPayRunPlanStore(MemoryPayRunPlanStore inner, PayRunPlanCache cache, int hits, int misses) : IPayRunPlanStore
{
    public MemoryPayRunPlanStore Inner { get; set; } = inner;
    public PayRunPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayRunPlan? Find(string code)
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

    public void Save(PayRunPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayRunPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay run plans. Empty fields match all.</summary>
public sealed record PayRunPlanQuery(string Unit, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(PayRunPlan item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayRunPlan> Apply(List<PayRunPlan> items)
    {
        var matched = Collections.Filter(items, (PayRunPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay run plan store.</summary>
public static class PayRunPlanStore
{
    public static IPayRunPlanStore NewMemoryPayRunPlanStore()
    {
        return new MemoryPayRunPlanStore(new Dictionary<string, PayRunPlan>(), new List<string>());
    }

    public static IPayRunPlanStore NewCachedPayRunPlanStore(MemoryPayRunPlanStore inner, int ttl, IClock clock)
    {
        return new CachedPayRunPlanStore(inner, PayRunPlanCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<PayRunPlan> FindPayRunPlans(IPayRunPlanStore store, PayRunPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
