namespace Acme.Workforce.Payrollops.RetroPay.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy retro pay plans by code.</summary>
public interface ILegacyRetroPayPlanStore
{
    LegacyRetroPayPlan? Find(string code);
    void Save(LegacyRetroPayPlan item);
    bool Remove(string code);
    List<LegacyRetroPayPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyRetroPayPlanStore(Dictionary<string, LegacyRetroPayPlan> items, List<string> order) : ILegacyRetroPayPlanStore
{
    public Dictionary<string, LegacyRetroPayPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyRetroPayPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyRetroPayPlan item)
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

    public List<LegacyRetroPayPlan> List()
    {
        var outValue = new List<LegacyRetroPayPlan>();
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
public sealed class CachedLegacyRetroPayPlanStore(MemoryLegacyRetroPayPlanStore inner, LegacyRetroPayPlanCache cache, int hits, int misses) : ILegacyRetroPayPlanStore
{
    public MemoryLegacyRetroPayPlanStore Inner { get; set; } = inner;
    public LegacyRetroPayPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyRetroPayPlan? Find(string code)
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

    public void Save(LegacyRetroPayPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyRetroPayPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy retro pay plans. Empty fields match all.</summary>
public sealed record LegacyRetroPayPlanQuery(string Source, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyRetroPayPlan item)
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

    public List<LegacyRetroPayPlan> Apply(List<LegacyRetroPayPlan> items)
    {
        var matched = Collections.Filter(items, (LegacyRetroPayPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy retro pay plan store.</summary>
public static class LegacyRetroPayPlanStore
{
    public static ILegacyRetroPayPlanStore NewMemoryLegacyRetroPayPlanStore()
    {
        return new MemoryLegacyRetroPayPlanStore(new Dictionary<string, LegacyRetroPayPlan>(), new List<string>());
    }

    public static ILegacyRetroPayPlanStore NewCachedLegacyRetroPayPlanStore(MemoryLegacyRetroPayPlanStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyRetroPayPlanStore(inner, LegacyRetroPayPlanCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<LegacyRetroPayPlan> FindLegacyRetroPayPlans(ILegacyRetroPayPlanStore store, LegacyRetroPayPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
