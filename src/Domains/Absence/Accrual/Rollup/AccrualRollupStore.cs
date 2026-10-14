namespace Acme.Workforce.Absence.Accrual.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps accrual rollups by code.</summary>
public interface IAccrualRollupStore
{
    AccrualRollup? Find(string code);
    void Save(AccrualRollup item);
    bool Remove(string code);
    List<AccrualRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccrualRollupStore(Dictionary<string, AccrualRollup> items, List<string> order) : IAccrualRollupStore
{
    public Dictionary<string, AccrualRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccrualRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccrualRollup item)
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

    public List<AccrualRollup> List()
    {
        var outValue = new List<AccrualRollup>();
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
public sealed class CachedAccrualRollupStore(MemoryAccrualRollupStore inner, AccrualRollupCache cache, int hits, int misses) : IAccrualRollupStore
{
    public MemoryAccrualRollupStore Inner { get; set; } = inner;
    public AccrualRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccrualRollup? Find(string code)
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

    public void Save(AccrualRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccrualRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects accrual rollups. Empty fields match all.</summary>
public sealed record AccrualRollupQuery(string Category, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(AccrualRollup item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AccrualRollup> Apply(List<AccrualRollup> items)
    {
        var matched = Collections.Filter(items, (AccrualRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of accrual rollup store.</summary>
public static class AccrualRollupStore
{
    public static IAccrualRollupStore NewMemoryAccrualRollupStore()
    {
        return new MemoryAccrualRollupStore(new Dictionary<string, AccrualRollup>(), new List<string>());
    }

    public static IAccrualRollupStore NewCachedAccrualRollupStore(MemoryAccrualRollupStore inner, int ttl, IClock clock)
    {
        return new CachedAccrualRollupStore(inner, AccrualRollupCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<AccrualRollup> FindAccrualRollups(IAccrualRollupStore store, AccrualRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
