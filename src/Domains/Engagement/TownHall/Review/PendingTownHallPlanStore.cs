namespace Acme.Workforce.Engagement.TownHall.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending town hall plans by code.</summary>
public interface IPendingTownHallPlanStore
{
    PendingTownHallPlan? Find(string code);
    void Save(PendingTownHallPlan item);
    bool Remove(string code);
    List<PendingTownHallPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingTownHallPlanStore(Dictionary<string, PendingTownHallPlan> items, List<string> order) : IPendingTownHallPlanStore
{
    public Dictionary<string, PendingTownHallPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingTownHallPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingTownHallPlan item)
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

    public List<PendingTownHallPlan> List()
    {
        var outValue = new List<PendingTownHallPlan>();
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
public sealed class CachedPendingTownHallPlanStore(MemoryPendingTownHallPlanStore inner, PendingTownHallPlanCache cache, int hits, int misses) : IPendingTownHallPlanStore
{
    public MemoryPendingTownHallPlanStore Inner { get; set; } = inner;
    public PendingTownHallPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingTownHallPlan? Find(string code)
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

    public void Save(PendingTownHallPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingTownHallPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending town hall plans. Empty fields match all.</summary>
public sealed record PendingTownHallPlanQuery(string Title, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(PendingTownHallPlan item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingTownHallPlan> Apply(List<PendingTownHallPlan> items)
    {
        var matched = Collections.Filter(items, (PendingTownHallPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending town hall plan store.</summary>
public static class PendingTownHallPlanStore
{
    public static IPendingTownHallPlanStore NewMemoryPendingTownHallPlanStore()
    {
        return new MemoryPendingTownHallPlanStore(new Dictionary<string, PendingTownHallPlan>(), new List<string>());
    }

    public static IPendingTownHallPlanStore NewCachedPendingTownHallPlanStore(MemoryPendingTownHallPlanStore inner, int ttl, IClock clock)
    {
        return new CachedPendingTownHallPlanStore(inner, PendingTownHallPlanCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PendingTownHallPlan> FindPendingTownHallPlans(IPendingTownHallPlanStore store, PendingTownHallPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
