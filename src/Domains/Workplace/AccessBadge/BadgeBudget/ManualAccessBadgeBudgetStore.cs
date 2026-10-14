namespace Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual access badge budgets by code.</summary>
public interface IManualAccessBadgeBudgetStore
{
    ManualAccessBadgeBudget? Find(string code);
    void Save(ManualAccessBadgeBudget item);
    bool Remove(string code);
    List<ManualAccessBadgeBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualAccessBadgeBudgetStore(Dictionary<string, ManualAccessBadgeBudget> items, List<string> order) : IManualAccessBadgeBudgetStore
{
    public Dictionary<string, ManualAccessBadgeBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualAccessBadgeBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualAccessBadgeBudget item)
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

    public List<ManualAccessBadgeBudget> List()
    {
        var outValue = new List<ManualAccessBadgeBudget>();
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
public sealed class CachedManualAccessBadgeBudgetStore(MemoryManualAccessBadgeBudgetStore inner, ManualAccessBadgeBudgetCache cache, int hits, int misses) : IManualAccessBadgeBudgetStore
{
    public MemoryManualAccessBadgeBudgetStore Inner { get; set; } = inner;
    public ManualAccessBadgeBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualAccessBadgeBudget? Find(string code)
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

    public void Save(ManualAccessBadgeBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualAccessBadgeBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual access badge budgets. Empty fields match all.</summary>
public sealed record ManualAccessBadgeBudgetQuery(string Unit, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(ManualAccessBadgeBudget item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualAccessBadgeBudget> Apply(List<ManualAccessBadgeBudget> items)
    {
        var matched = Collections.Filter(items, (ManualAccessBadgeBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual access badge budget store.</summary>
public static class ManualAccessBadgeBudgetStore
{
    public static IManualAccessBadgeBudgetStore NewMemoryManualAccessBadgeBudgetStore()
    {
        return new MemoryManualAccessBadgeBudgetStore(
            new Dictionary<string, ManualAccessBadgeBudget>(),
            new List<string>());
    }

    public static IManualAccessBadgeBudgetStore NewCachedManualAccessBadgeBudgetStore(MemoryManualAccessBadgeBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedManualAccessBadgeBudgetStore(
            inner,
            ManualAccessBadgeBudgetCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<ManualAccessBadgeBudget> FindManualAccessBadgeBudgets(IManualAccessBadgeBudgetStore store, ManualAccessBadgeBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
