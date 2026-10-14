namespace Acme.Workforce.Compensation.MeritCycle.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps merit cycle plans by code.</summary>
public interface IMeritCyclePlanStore
{
    MeritCyclePlan? Find(string code);
    void Save(MeritCyclePlan item);
    bool Remove(string code);
    List<MeritCyclePlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMeritCyclePlanStore(Dictionary<string, MeritCyclePlan> items, List<string> order) : IMeritCyclePlanStore
{
    public Dictionary<string, MeritCyclePlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MeritCyclePlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MeritCyclePlan item)
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

    public List<MeritCyclePlan> List()
    {
        var outValue = new List<MeritCyclePlan>();
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
public sealed class CachedMeritCyclePlanStore(MemoryMeritCyclePlanStore inner, MeritCyclePlanCache cache, int hits, int misses) : IMeritCyclePlanStore
{
    public MemoryMeritCyclePlanStore Inner { get; set; } = inner;
    public MeritCyclePlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MeritCyclePlan? Find(string code)
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

    public void Save(MeritCyclePlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MeritCyclePlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects merit cycle plans. Empty fields match all.</summary>
public sealed record MeritCyclePlanQuery(string Unit, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(MeritCyclePlan item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
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

    public List<MeritCyclePlan> Apply(List<MeritCyclePlan> items)
    {
        var matched = Collections.Filter(items, (MeritCyclePlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of merit cycle plan store.</summary>
public static class MeritCyclePlanStore
{
    public static IMeritCyclePlanStore NewMemoryMeritCyclePlanStore()
    {
        return new MemoryMeritCyclePlanStore(new Dictionary<string, MeritCyclePlan>(), new List<string>());
    }

    public static IMeritCyclePlanStore NewCachedMeritCyclePlanStore(MemoryMeritCyclePlanStore inner, int ttl, IClock clock)
    {
        return new CachedMeritCyclePlanStore(inner, MeritCyclePlanCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<MeritCyclePlan> FindMeritCyclePlans(IMeritCyclePlanStore store, MeritCyclePlanQuery query)
    {
        return query.Apply(store.List());
    }
}
