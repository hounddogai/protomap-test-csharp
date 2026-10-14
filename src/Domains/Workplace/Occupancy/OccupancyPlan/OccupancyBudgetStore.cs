namespace Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps occupancy budgets by code.</summary>
public interface IOccupancyBudgetStore
{
    OccupancyBudget? Find(string code);
    void Save(OccupancyBudget item);
    bool Remove(string code);
    List<OccupancyBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOccupancyBudgetStore(Dictionary<string, OccupancyBudget> items, List<string> order) : IOccupancyBudgetStore
{
    public Dictionary<string, OccupancyBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OccupancyBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OccupancyBudget item)
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

    public List<OccupancyBudget> List()
    {
        var outValue = new List<OccupancyBudget>();
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
public sealed class CachedOccupancyBudgetStore(MemoryOccupancyBudgetStore inner, OccupancyBudgetCache cache, int hits, int misses) : IOccupancyBudgetStore
{
    public MemoryOccupancyBudgetStore Inner { get; set; } = inner;
    public OccupancyBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OccupancyBudget? Find(string code)
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

    public void Save(OccupancyBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OccupancyBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects occupancy budgets. Empty fields match all.</summary>
public sealed record OccupancyBudgetQuery(string Label, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(OccupancyBudget item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<OccupancyBudget> Apply(List<OccupancyBudget> items)
    {
        var matched = Collections.Filter(items, (OccupancyBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of occupancy budget store.</summary>
public static class OccupancyBudgetStore
{
    public static IOccupancyBudgetStore NewMemoryOccupancyBudgetStore()
    {
        return new MemoryOccupancyBudgetStore(new Dictionary<string, OccupancyBudget>(), new List<string>());
    }

    public static IOccupancyBudgetStore NewCachedOccupancyBudgetStore(MemoryOccupancyBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedOccupancyBudgetStore(inner, OccupancyBudgetCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<OccupancyBudget> FindOccupancyBudgets(IOccupancyBudgetStore store, OccupancyBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
