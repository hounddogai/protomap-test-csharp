namespace Acme.Workforce.Compensation.MeritCycle.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps merit cycle estimates by code.</summary>
public interface IMeritCycleEstimateStore
{
    MeritCycleEstimate? Find(string code);
    void Save(MeritCycleEstimate item);
    bool Remove(string code);
    List<MeritCycleEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMeritCycleEstimateStore(Dictionary<string, MeritCycleEstimate> items, List<string> order) : IMeritCycleEstimateStore
{
    public Dictionary<string, MeritCycleEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MeritCycleEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MeritCycleEstimate item)
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

    public List<MeritCycleEstimate> List()
    {
        var outValue = new List<MeritCycleEstimate>();
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
public sealed class CachedMeritCycleEstimateStore(MemoryMeritCycleEstimateStore inner, MeritCycleEstimateCache cache, int hits, int misses) : IMeritCycleEstimateStore
{
    public MemoryMeritCycleEstimateStore Inner { get; set; } = inner;
    public MeritCycleEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MeritCycleEstimate? Find(string code)
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

    public void Save(MeritCycleEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MeritCycleEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects merit cycle estimates. Empty fields match all.</summary>
public sealed record MeritCycleEstimateQuery(string Channel, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(MeritCycleEstimate item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<MeritCycleEstimate> Apply(List<MeritCycleEstimate> items)
    {
        var matched = Collections.Filter(items, (MeritCycleEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of merit cycle estimate store.</summary>
public static class MeritCycleEstimateStore
{
    public static IMeritCycleEstimateStore NewMemoryMeritCycleEstimateStore()
    {
        return new MemoryMeritCycleEstimateStore(new Dictionary<string, MeritCycleEstimate>(), new List<string>());
    }

    public static IMeritCycleEstimateStore NewCachedMeritCycleEstimateStore(MemoryMeritCycleEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedMeritCycleEstimateStore(inner, MeritCycleEstimateCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<MeritCycleEstimate> FindMeritCycleEstimates(IMeritCycleEstimateStore store, MeritCycleEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
