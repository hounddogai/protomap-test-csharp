namespace Acme.Workforce.Compensation.MeritCycle.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict merit cycle plans by code.</summary>
public interface IStrictMeritCyclePlanStore
{
    StrictMeritCyclePlan? Find(string code);
    void Save(StrictMeritCyclePlan item);
    bool Remove(string code);
    List<StrictMeritCyclePlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictMeritCyclePlanStore(Dictionary<string, StrictMeritCyclePlan> items, List<string> order) : IStrictMeritCyclePlanStore
{
    public Dictionary<string, StrictMeritCyclePlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictMeritCyclePlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictMeritCyclePlan item)
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

    public List<StrictMeritCyclePlan> List()
    {
        var outValue = new List<StrictMeritCyclePlan>();
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
public sealed class CachedStrictMeritCyclePlanStore(MemoryStrictMeritCyclePlanStore inner, StrictMeritCyclePlanCache cache, int hits, int misses) : IStrictMeritCyclePlanStore
{
    public MemoryStrictMeritCyclePlanStore Inner { get; set; } = inner;
    public StrictMeritCyclePlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictMeritCyclePlan? Find(string code)
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

    public void Save(StrictMeritCyclePlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictMeritCyclePlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict merit cycle plans. Empty fields match all.</summary>
public sealed record StrictMeritCyclePlanQuery(string Channel, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(StrictMeritCyclePlan item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictMeritCyclePlan> Apply(List<StrictMeritCyclePlan> items)
    {
        var matched = Collections.Filter(items, (StrictMeritCyclePlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict merit cycle plan store.</summary>
public static class StrictMeritCyclePlanStore
{
    public static IStrictMeritCyclePlanStore NewMemoryStrictMeritCyclePlanStore()
    {
        return new MemoryStrictMeritCyclePlanStore(
            new Dictionary<string, StrictMeritCyclePlan>(),
            new List<string>());
    }

    public static IStrictMeritCyclePlanStore NewCachedStrictMeritCyclePlanStore(MemoryStrictMeritCyclePlanStore inner, int ttl, IClock clock)
    {
        return new CachedStrictMeritCyclePlanStore(inner, StrictMeritCyclePlanCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<StrictMeritCyclePlan> FindStrictMeritCyclePlans(IStrictMeritCyclePlanStore store, StrictMeritCyclePlanQuery query)
    {
        return query.Apply(store.List());
    }
}
