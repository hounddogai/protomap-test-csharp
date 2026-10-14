namespace Acme.Workforce.Talent.SuccessionPlan.PlanTarget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending succession plan targets by code.</summary>
public interface IPendingSuccessionPlanTargetStore
{
    PendingSuccessionPlanTarget? Find(string code);
    void Save(PendingSuccessionPlanTarget item);
    bool Remove(string code);
    List<PendingSuccessionPlanTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingSuccessionPlanTargetStore(Dictionary<string, PendingSuccessionPlanTarget> items, List<string> order) : IPendingSuccessionPlanTargetStore
{
    public Dictionary<string, PendingSuccessionPlanTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingSuccessionPlanTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingSuccessionPlanTarget item)
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

    public List<PendingSuccessionPlanTarget> List()
    {
        var outValue = new List<PendingSuccessionPlanTarget>();
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
public sealed class CachedPendingSuccessionPlanTargetStore(MemoryPendingSuccessionPlanTargetStore inner, PendingSuccessionPlanTargetCache cache, int hits, int misses) : IPendingSuccessionPlanTargetStore
{
    public MemoryPendingSuccessionPlanTargetStore Inner { get; set; } = inner;
    public PendingSuccessionPlanTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingSuccessionPlanTarget? Find(string code)
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

    public void Save(PendingSuccessionPlanTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingSuccessionPlanTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending succession plan targets. Empty fields match all.</summary>
public sealed record PendingSuccessionPlanTargetQuery(string Label, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(PendingSuccessionPlanTarget item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingSuccessionPlanTarget> Apply(List<PendingSuccessionPlanTarget> items)
    {
        var matched = Collections.Filter(items, (PendingSuccessionPlanTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending succession plan target store.</summary>
public static class PendingSuccessionPlanTargetStore
{
    public static IPendingSuccessionPlanTargetStore NewMemoryPendingSuccessionPlanTargetStore()
    {
        return new MemoryPendingSuccessionPlanTargetStore(
            new Dictionary<string, PendingSuccessionPlanTarget>(),
            new List<string>());
    }

    public static IPendingSuccessionPlanTargetStore NewCachedPendingSuccessionPlanTargetStore(MemoryPendingSuccessionPlanTargetStore inner, int ttl, IClock clock)
    {
        return new CachedPendingSuccessionPlanTargetStore(
            inner,
            PendingSuccessionPlanTargetCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<PendingSuccessionPlanTarget> FindPendingSuccessionPlanTargets(IPendingSuccessionPlanTargetStore store, PendingSuccessionPlanTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
