namespace Acme.Workforce.Compensation.MeritCycle.CycleTarget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps merit cycle budgets by code.</summary>
public interface IMeritCycleBudgetStore
{
    MeritCycleBudget? Find(string code);
    void Save(MeritCycleBudget item);
    bool Remove(string code);
    List<MeritCycleBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMeritCycleBudgetStore(Dictionary<string, MeritCycleBudget> items, List<string> order) : IMeritCycleBudgetStore
{
    public Dictionary<string, MeritCycleBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MeritCycleBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MeritCycleBudget item)
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

    public List<MeritCycleBudget> List()
    {
        var outValue = new List<MeritCycleBudget>();
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
public sealed class CachedMeritCycleBudgetStore(MemoryMeritCycleBudgetStore inner, MeritCycleBudgetCache cache, int hits, int misses) : IMeritCycleBudgetStore
{
    public MemoryMeritCycleBudgetStore Inner { get; set; } = inner;
    public MeritCycleBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MeritCycleBudget? Find(string code)
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

    public void Save(MeritCycleBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MeritCycleBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects merit cycle budgets. Empty fields match all.</summary>
public sealed record MeritCycleBudgetQuery(string Department, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(MeritCycleBudget item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<MeritCycleBudget> Apply(List<MeritCycleBudget> items)
    {
        var matched = Collections.Filter(items, (MeritCycleBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of merit cycle budget store.</summary>
public static class MeritCycleBudgetStore
{
    public static IMeritCycleBudgetStore NewMemoryMeritCycleBudgetStore()
    {
        return new MemoryMeritCycleBudgetStore(new Dictionary<string, MeritCycleBudget>(), new List<string>());
    }

    public static IMeritCycleBudgetStore NewCachedMeritCycleBudgetStore(MemoryMeritCycleBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedMeritCycleBudgetStore(inner, MeritCycleBudgetCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<MeritCycleBudget> FindMeritCycleBudgets(IMeritCycleBudgetStore store, MeritCycleBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
