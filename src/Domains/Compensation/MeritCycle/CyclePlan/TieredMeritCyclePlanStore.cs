namespace Acme.Workforce.Compensation.MeritCycle.CyclePlan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered merit cycle plans by code.</summary>
public interface ITieredMeritCyclePlanStore
{
    TieredMeritCyclePlan? Find(string code);
    void Save(TieredMeritCyclePlan item);
    bool Remove(string code);
    List<TieredMeritCyclePlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredMeritCyclePlanStore(Dictionary<string, TieredMeritCyclePlan> items, List<string> order) : ITieredMeritCyclePlanStore
{
    public Dictionary<string, TieredMeritCyclePlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredMeritCyclePlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredMeritCyclePlan item)
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

    public List<TieredMeritCyclePlan> List()
    {
        var outValue = new List<TieredMeritCyclePlan>();
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
public sealed class CachedTieredMeritCyclePlanStore(MemoryTieredMeritCyclePlanStore inner, TieredMeritCyclePlanCache cache, int hits, int misses) : ITieredMeritCyclePlanStore
{
    public MemoryTieredMeritCyclePlanStore Inner { get; set; } = inner;
    public TieredMeritCyclePlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredMeritCyclePlan? Find(string code)
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

    public void Save(TieredMeritCyclePlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredMeritCyclePlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered merit cycle plans. Empty fields match all.</summary>
public sealed record TieredMeritCyclePlanQuery(string Department, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(TieredMeritCyclePlan item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredMeritCyclePlan> Apply(List<TieredMeritCyclePlan> items)
    {
        var matched = Collections.Filter(items, (TieredMeritCyclePlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered merit cycle plan store.</summary>
public static class TieredMeritCyclePlanStore
{
    public static ITieredMeritCyclePlanStore NewMemoryTieredMeritCyclePlanStore()
    {
        return new MemoryTieredMeritCyclePlanStore(
            new Dictionary<string, TieredMeritCyclePlan>(),
            new List<string>());
    }

    public static ITieredMeritCyclePlanStore NewCachedTieredMeritCyclePlanStore(MemoryTieredMeritCyclePlanStore inner, int ttl, IClock clock)
    {
        return new CachedTieredMeritCyclePlanStore(inner, TieredMeritCyclePlanCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<TieredMeritCyclePlan> FindTieredMeritCyclePlans(ITieredMeritCyclePlanStore store, TieredMeritCyclePlanQuery query)
    {
        return query.Apply(store.List());
    }
}
