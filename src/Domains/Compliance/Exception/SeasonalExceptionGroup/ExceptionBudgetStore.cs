namespace Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps exception budgets by code.</summary>
public interface IExceptionBudgetStore
{
    ExceptionBudget? Find(string code);
    void Save(ExceptionBudget item);
    bool Remove(string code);
    List<ExceptionBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryExceptionBudgetStore(Dictionary<string, ExceptionBudget> items, List<string> order) : IExceptionBudgetStore
{
    public Dictionary<string, ExceptionBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ExceptionBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ExceptionBudget item)
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

    public List<ExceptionBudget> List()
    {
        var outValue = new List<ExceptionBudget>();
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
public sealed class CachedExceptionBudgetStore(MemoryExceptionBudgetStore inner, ExceptionBudgetCache cache, int hits, int misses) : IExceptionBudgetStore
{
    public MemoryExceptionBudgetStore Inner { get; set; } = inner;
    public ExceptionBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ExceptionBudget? Find(string code)
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

    public void Save(ExceptionBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ExceptionBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects exception budgets. Empty fields match all.</summary>
public sealed record ExceptionBudgetQuery(string Segment, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(ExceptionBudget item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ExceptionBudget> Apply(List<ExceptionBudget> items)
    {
        var matched = Collections.Filter(items, (ExceptionBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of exception budget store.</summary>
public static class ExceptionBudgetStore
{
    public static IExceptionBudgetStore NewMemoryExceptionBudgetStore()
    {
        return new MemoryExceptionBudgetStore(new Dictionary<string, ExceptionBudget>(), new List<string>());
    }

    public static IExceptionBudgetStore NewCachedExceptionBudgetStore(MemoryExceptionBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedExceptionBudgetStore(inner, ExceptionBudgetCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<ExceptionBudget> FindExceptionBudgets(IExceptionBudgetStore store, ExceptionBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
