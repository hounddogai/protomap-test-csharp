namespace Acme.Workforce.Absence.ReturnPlan.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending return plan rollups by code.</summary>
public interface IPendingReturnPlanRollupStore
{
    PendingReturnPlanRollup? Find(string code);
    void Save(PendingReturnPlanRollup item);
    bool Remove(string code);
    List<PendingReturnPlanRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingReturnPlanRollupStore(Dictionary<string, PendingReturnPlanRollup> items, List<string> order) : IPendingReturnPlanRollupStore
{
    public Dictionary<string, PendingReturnPlanRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingReturnPlanRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingReturnPlanRollup item)
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

    public List<PendingReturnPlanRollup> List()
    {
        var outValue = new List<PendingReturnPlanRollup>();
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
public sealed class CachedPendingReturnPlanRollupStore(MemoryPendingReturnPlanRollupStore inner, PendingReturnPlanRollupCache cache, int hits, int misses) : IPendingReturnPlanRollupStore
{
    public MemoryPendingReturnPlanRollupStore Inner { get; set; } = inner;
    public PendingReturnPlanRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingReturnPlanRollup? Find(string code)
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

    public void Save(PendingReturnPlanRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingReturnPlanRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending return plan rollups. Empty fields match all.</summary>
public sealed record PendingReturnPlanRollupQuery(string Title, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(PendingReturnPlanRollup item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingReturnPlanRollup> Apply(List<PendingReturnPlanRollup> items)
    {
        var matched = Collections.Filter(items, (PendingReturnPlanRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending return plan rollup store.</summary>
public static class PendingReturnPlanRollupStore
{
    public static IPendingReturnPlanRollupStore NewMemoryPendingReturnPlanRollupStore()
    {
        return new MemoryPendingReturnPlanRollupStore(
            new Dictionary<string, PendingReturnPlanRollup>(),
            new List<string>());
    }

    public static IPendingReturnPlanRollupStore NewCachedPendingReturnPlanRollupStore(MemoryPendingReturnPlanRollupStore inner, int ttl, IClock clock)
    {
        return new CachedPendingReturnPlanRollupStore(
            inner,
            PendingReturnPlanRollupCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<PendingReturnPlanRollup> FindPendingReturnPlanRollups(IPendingReturnPlanRollupStore store, PendingReturnPlanRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
