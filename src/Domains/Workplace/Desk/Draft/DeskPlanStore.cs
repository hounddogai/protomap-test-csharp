namespace Acme.Workforce.Workplace.Desk.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps desk plans by code.</summary>
public interface IDeskPlanStore
{
    DeskPlan? Find(string code);
    void Save(DeskPlan item);
    bool Remove(string code);
    List<DeskPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeskPlanStore(Dictionary<string, DeskPlan> items, List<string> order) : IDeskPlanStore
{
    public Dictionary<string, DeskPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeskPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeskPlan item)
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

    public List<DeskPlan> List()
    {
        var outValue = new List<DeskPlan>();
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
public sealed class CachedDeskPlanStore(MemoryDeskPlanStore inner, DeskPlanCache cache, int hits, int misses) : IDeskPlanStore
{
    public MemoryDeskPlanStore Inner { get; set; } = inner;
    public DeskPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeskPlan? Find(string code)
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

    public void Save(DeskPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeskPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects desk plans. Empty fields match all.</summary>
public sealed record DeskPlanQuery(string Title, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(DeskPlan item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<DeskPlan> Apply(List<DeskPlan> items)
    {
        var matched = Collections.Filter(items, (DeskPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of desk plan store.</summary>
public static class DeskPlanStore
{
    public static IDeskPlanStore NewMemoryDeskPlanStore()
    {
        return new MemoryDeskPlanStore(new Dictionary<string, DeskPlan>(), new List<string>());
    }

    public static IDeskPlanStore NewCachedDeskPlanStore(MemoryDeskPlanStore inner, int ttl, IClock clock)
    {
        return new CachedDeskPlanStore(inner, DeskPlanCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<DeskPlan> FindDeskPlans(IDeskPlanStore store, DeskPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
