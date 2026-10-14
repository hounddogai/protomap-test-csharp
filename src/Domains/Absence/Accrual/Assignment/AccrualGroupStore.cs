namespace Acme.Workforce.Absence.Accrual.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps accrual groups by code.</summary>
public interface IAccrualGroupStore
{
    AccrualGroup? Find(string code);
    void Save(AccrualGroup item);
    bool Remove(string code);
    List<AccrualGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccrualGroupStore(Dictionary<string, AccrualGroup> items, List<string> order) : IAccrualGroupStore
{
    public Dictionary<string, AccrualGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccrualGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccrualGroup item)
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

    public List<AccrualGroup> List()
    {
        var outValue = new List<AccrualGroup>();
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
public sealed class CachedAccrualGroupStore(MemoryAccrualGroupStore inner, AccrualGroupCache cache, int hits, int misses) : IAccrualGroupStore
{
    public MemoryAccrualGroupStore Inner { get; set; } = inner;
    public AccrualGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccrualGroup? Find(string code)
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

    public void Save(AccrualGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccrualGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects accrual groups. Empty fields match all.</summary>
public sealed record AccrualGroupQuery(string Source, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(AccrualGroup item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AccrualGroup> Apply(List<AccrualGroup> items)
    {
        var matched = Collections.Filter(items, (AccrualGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of accrual group store.</summary>
public static class AccrualGroupStore
{
    public static IAccrualGroupStore NewMemoryAccrualGroupStore()
    {
        return new MemoryAccrualGroupStore(new Dictionary<string, AccrualGroup>(), new List<string>());
    }

    public static IAccrualGroupStore NewCachedAccrualGroupStore(MemoryAccrualGroupStore inner, int ttl, IClock clock)
    {
        return new CachedAccrualGroupStore(inner, AccrualGroupCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<AccrualGroup> FindAccrualGroups(IAccrualGroupStore store, AccrualGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
