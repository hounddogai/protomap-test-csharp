namespace Acme.Workforce.Payrollops.Garnishment.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient garnishment budgets by code.</summary>
public interface ILenientGarnishmentBudgetStore
{
    LenientGarnishmentBudget? Find(string code);
    void Save(LenientGarnishmentBudget item);
    bool Remove(string code);
    List<LenientGarnishmentBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientGarnishmentBudgetStore(Dictionary<string, LenientGarnishmentBudget> items, List<string> order) : ILenientGarnishmentBudgetStore
{
    public Dictionary<string, LenientGarnishmentBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientGarnishmentBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientGarnishmentBudget item)
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

    public List<LenientGarnishmentBudget> List()
    {
        var outValue = new List<LenientGarnishmentBudget>();
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
public sealed class CachedLenientGarnishmentBudgetStore(MemoryLenientGarnishmentBudgetStore inner, LenientGarnishmentBudgetCache cache, int hits, int misses) : ILenientGarnishmentBudgetStore
{
    public MemoryLenientGarnishmentBudgetStore Inner { get; set; } = inner;
    public LenientGarnishmentBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientGarnishmentBudget? Find(string code)
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

    public void Save(LenientGarnishmentBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientGarnishmentBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient garnishment budgets. Empty fields match all.</summary>
public sealed record LenientGarnishmentBudgetQuery(string Label, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(LenientGarnishmentBudget item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientGarnishmentBudget> Apply(List<LenientGarnishmentBudget> items)
    {
        var matched = Collections.Filter(items, (LenientGarnishmentBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient garnishment budget store.</summary>
public static class LenientGarnishmentBudgetStore
{
    public static ILenientGarnishmentBudgetStore NewMemoryLenientGarnishmentBudgetStore()
    {
        return new MemoryLenientGarnishmentBudgetStore(
            new Dictionary<string, LenientGarnishmentBudget>(),
            new List<string>());
    }

    public static ILenientGarnishmentBudgetStore NewCachedLenientGarnishmentBudgetStore(MemoryLenientGarnishmentBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedLenientGarnishmentBudgetStore(
            inner,
            LenientGarnishmentBudgetCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<LenientGarnishmentBudget> FindLenientGarnishmentBudgets(ILenientGarnishmentBudgetStore store, LenientGarnishmentBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
