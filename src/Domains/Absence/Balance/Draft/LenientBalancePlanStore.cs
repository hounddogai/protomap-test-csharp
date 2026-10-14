namespace Acme.Workforce.Absence.Balance.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient balance plans by code.</summary>
public interface ILenientBalancePlanStore
{
    LenientBalancePlan? Find(string code);
    void Save(LenientBalancePlan item);
    bool Remove(string code);
    List<LenientBalancePlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientBalancePlanStore(Dictionary<string, LenientBalancePlan> items, List<string> order) : ILenientBalancePlanStore
{
    public Dictionary<string, LenientBalancePlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientBalancePlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientBalancePlan item)
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

    public List<LenientBalancePlan> List()
    {
        var outValue = new List<LenientBalancePlan>();
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
public sealed class CachedLenientBalancePlanStore(MemoryLenientBalancePlanStore inner, LenientBalancePlanCache cache, int hits, int misses) : ILenientBalancePlanStore
{
    public MemoryLenientBalancePlanStore Inner { get; set; } = inner;
    public LenientBalancePlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientBalancePlan? Find(string code)
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

    public void Save(LenientBalancePlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientBalancePlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient balance plans. Empty fields match all.</summary>
public sealed record LenientBalancePlanQuery(string Name, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(LenientBalancePlan item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientBalancePlan> Apply(List<LenientBalancePlan> items)
    {
        var matched = Collections.Filter(items, (LenientBalancePlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient balance plan store.</summary>
public static class LenientBalancePlanStore
{
    public static ILenientBalancePlanStore NewMemoryLenientBalancePlanStore()
    {
        return new MemoryLenientBalancePlanStore(new Dictionary<string, LenientBalancePlan>(), new List<string>());
    }

    public static ILenientBalancePlanStore NewCachedLenientBalancePlanStore(MemoryLenientBalancePlanStore inner, int ttl, IClock clock)
    {
        return new CachedLenientBalancePlanStore(inner, LenientBalancePlanCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<LenientBalancePlan> FindLenientBalancePlans(ILenientBalancePlanStore store, LenientBalancePlanQuery query)
    {
        return query.Apply(store.List());
    }
}
