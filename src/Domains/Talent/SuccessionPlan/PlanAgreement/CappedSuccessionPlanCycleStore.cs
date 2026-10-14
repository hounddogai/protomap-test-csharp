namespace Acme.Workforce.Talent.SuccessionPlan.PlanAgreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped succession plan cycles by code.</summary>
public interface ICappedSuccessionPlanCycleStore
{
    CappedSuccessionPlanCycle? Find(string code);
    void Save(CappedSuccessionPlanCycle item);
    bool Remove(string code);
    List<CappedSuccessionPlanCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedSuccessionPlanCycleStore(Dictionary<string, CappedSuccessionPlanCycle> items, List<string> order) : ICappedSuccessionPlanCycleStore
{
    public Dictionary<string, CappedSuccessionPlanCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedSuccessionPlanCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedSuccessionPlanCycle item)
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

    public List<CappedSuccessionPlanCycle> List()
    {
        var outValue = new List<CappedSuccessionPlanCycle>();
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
public sealed class CachedCappedSuccessionPlanCycleStore(MemoryCappedSuccessionPlanCycleStore inner, CappedSuccessionPlanCycleCache cache, int hits, int misses) : ICappedSuccessionPlanCycleStore
{
    public MemoryCappedSuccessionPlanCycleStore Inner { get; set; } = inner;
    public CappedSuccessionPlanCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedSuccessionPlanCycle? Find(string code)
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

    public void Save(CappedSuccessionPlanCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedSuccessionPlanCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped succession plan cycles. Empty fields match all.</summary>
public sealed record CappedSuccessionPlanCycleQuery(string Channel, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(CappedSuccessionPlanCycle item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedSuccessionPlanCycle> Apply(List<CappedSuccessionPlanCycle> items)
    {
        var matched = Collections.Filter(items, (CappedSuccessionPlanCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped succession plan cycle store.</summary>
public static class CappedSuccessionPlanCycleStore
{
    public static ICappedSuccessionPlanCycleStore NewMemoryCappedSuccessionPlanCycleStore()
    {
        return new MemoryCappedSuccessionPlanCycleStore(
            new Dictionary<string, CappedSuccessionPlanCycle>(),
            new List<string>());
    }

    public static ICappedSuccessionPlanCycleStore NewCachedCappedSuccessionPlanCycleStore(MemoryCappedSuccessionPlanCycleStore inner, int ttl, IClock clock)
    {
        return new CachedCappedSuccessionPlanCycleStore(
            inner,
            CappedSuccessionPlanCycleCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<CappedSuccessionPlanCycle> FindCappedSuccessionPlanCycles(ICappedSuccessionPlanCycleStore store, CappedSuccessionPlanCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
