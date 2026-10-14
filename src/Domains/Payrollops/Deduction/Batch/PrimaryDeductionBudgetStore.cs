namespace Acme.Workforce.Payrollops.Deduction.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary deduction budgets by code.</summary>
public interface IPrimaryDeductionBudgetStore
{
    PrimaryDeductionBudget? Find(string code);
    void Save(PrimaryDeductionBudget item);
    bool Remove(string code);
    List<PrimaryDeductionBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryDeductionBudgetStore(Dictionary<string, PrimaryDeductionBudget> items, List<string> order) : IPrimaryDeductionBudgetStore
{
    public Dictionary<string, PrimaryDeductionBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryDeductionBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryDeductionBudget item)
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

    public List<PrimaryDeductionBudget> List()
    {
        var outValue = new List<PrimaryDeductionBudget>();
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
public sealed class CachedPrimaryDeductionBudgetStore(MemoryPrimaryDeductionBudgetStore inner, PrimaryDeductionBudgetCache cache, int hits, int misses) : IPrimaryDeductionBudgetStore
{
    public MemoryPrimaryDeductionBudgetStore Inner { get; set; } = inner;
    public PrimaryDeductionBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryDeductionBudget? Find(string code)
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

    public void Save(PrimaryDeductionBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryDeductionBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary deduction budgets. Empty fields match all.</summary>
public sealed record PrimaryDeductionBudgetQuery(string Source, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryDeductionBudget item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryDeductionBudget> Apply(List<PrimaryDeductionBudget> items)
    {
        var matched = Collections.Filter(items, (PrimaryDeductionBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary deduction budget store.</summary>
public static class PrimaryDeductionBudgetStore
{
    public static IPrimaryDeductionBudgetStore NewMemoryPrimaryDeductionBudgetStore()
    {
        return new MemoryPrimaryDeductionBudgetStore(
            new Dictionary<string, PrimaryDeductionBudget>(),
            new List<string>());
    }

    public static IPrimaryDeductionBudgetStore NewCachedPrimaryDeductionBudgetStore(MemoryPrimaryDeductionBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryDeductionBudgetStore(
            inner,
            PrimaryDeductionBudgetCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryDeductionBudget> FindPrimaryDeductionBudgets(IPrimaryDeductionBudgetStore store, PrimaryDeductionBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
