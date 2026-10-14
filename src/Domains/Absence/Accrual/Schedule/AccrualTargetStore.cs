namespace Acme.Workforce.Absence.Accrual.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps accrual targets by code.</summary>
public interface IAccrualTargetStore
{
    AccrualTarget? Find(string code);
    void Save(AccrualTarget item);
    bool Remove(string code);
    List<AccrualTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccrualTargetStore(Dictionary<string, AccrualTarget> items, List<string> order) : IAccrualTargetStore
{
    public Dictionary<string, AccrualTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccrualTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccrualTarget item)
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

    public List<AccrualTarget> List()
    {
        var outValue = new List<AccrualTarget>();
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
public sealed class CachedAccrualTargetStore(MemoryAccrualTargetStore inner, AccrualTargetCache cache, int hits, int misses) : IAccrualTargetStore
{
    public MemoryAccrualTargetStore Inner { get; set; } = inner;
    public AccrualTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccrualTarget? Find(string code)
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

    public void Save(AccrualTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccrualTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects accrual targets. Empty fields match all.</summary>
public sealed record AccrualTargetQuery(string Region, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(AccrualTarget item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AccrualTarget> Apply(List<AccrualTarget> items)
    {
        var matched = Collections.Filter(items, (AccrualTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of accrual target store.</summary>
public static class AccrualTargetStore
{
    public static IAccrualTargetStore NewMemoryAccrualTargetStore()
    {
        return new MemoryAccrualTargetStore(new Dictionary<string, AccrualTarget>(), new List<string>());
    }

    public static IAccrualTargetStore NewCachedAccrualTargetStore(MemoryAccrualTargetStore inner, int ttl, IClock clock)
    {
        return new CachedAccrualTargetStore(inner, AccrualTargetCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<AccrualTarget> FindAccrualTargets(IAccrualTargetStore store, AccrualTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
