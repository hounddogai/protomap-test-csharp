namespace Acme.Workforce.Talent.SuccessionPlan.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps succession plan cycles by code.</summary>
public interface ISuccessionPlanCycleStore
{
    SuccessionPlanCycle? Find(string code);
    void Save(SuccessionPlanCycle item);
    bool Remove(string code);
    List<SuccessionPlanCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySuccessionPlanCycleStore(Dictionary<string, SuccessionPlanCycle> items, List<string> order) : ISuccessionPlanCycleStore
{
    public Dictionary<string, SuccessionPlanCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SuccessionPlanCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SuccessionPlanCycle item)
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

    public List<SuccessionPlanCycle> List()
    {
        var outValue = new List<SuccessionPlanCycle>();
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
public sealed class CachedSuccessionPlanCycleStore(MemorySuccessionPlanCycleStore inner, SuccessionPlanCycleCache cache, int hits, int misses) : ISuccessionPlanCycleStore
{
    public MemorySuccessionPlanCycleStore Inner { get; set; } = inner;
    public SuccessionPlanCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SuccessionPlanCycle? Find(string code)
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

    public void Save(SuccessionPlanCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SuccessionPlanCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects succession plan cycles. Empty fields match all.</summary>
public sealed record SuccessionPlanCycleQuery(string Locale, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(SuccessionPlanCycle item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SuccessionPlanCycle> Apply(List<SuccessionPlanCycle> items)
    {
        var matched = Collections.Filter(items, (SuccessionPlanCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of succession plan cycle store.</summary>
public static class SuccessionPlanCycleStore
{
    public static ISuccessionPlanCycleStore NewMemorySuccessionPlanCycleStore()
    {
        return new MemorySuccessionPlanCycleStore(new Dictionary<string, SuccessionPlanCycle>(), new List<string>());
    }

    public static ISuccessionPlanCycleStore NewCachedSuccessionPlanCycleStore(MemorySuccessionPlanCycleStore inner, int ttl, IClock clock)
    {
        return new CachedSuccessionPlanCycleStore(inner, SuccessionPlanCycleCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<SuccessionPlanCycle> FindSuccessionPlanCycles(ISuccessionPlanCycleStore store, SuccessionPlanCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
