namespace Acme.Workforce.Orgdesign.Position.RollingPositionEstimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient position estimates by code.</summary>
public interface ILenientPositionEstimateStore
{
    LenientPositionEstimate? Find(string code);
    void Save(LenientPositionEstimate item);
    bool Remove(string code);
    List<LenientPositionEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientPositionEstimateStore(Dictionary<string, LenientPositionEstimate> items, List<string> order) : ILenientPositionEstimateStore
{
    public Dictionary<string, LenientPositionEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientPositionEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientPositionEstimate item)
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

    public List<LenientPositionEstimate> List()
    {
        var outValue = new List<LenientPositionEstimate>();
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
public sealed class CachedLenientPositionEstimateStore(MemoryLenientPositionEstimateStore inner, LenientPositionEstimateCache cache, int hits, int misses) : ILenientPositionEstimateStore
{
    public MemoryLenientPositionEstimateStore Inner { get; set; } = inner;
    public LenientPositionEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientPositionEstimate? Find(string code)
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

    public void Save(LenientPositionEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientPositionEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient position estimates. Empty fields match all.</summary>
public sealed record LenientPositionEstimateQuery(string Unit, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(LenientPositionEstimate item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
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

    public List<LenientPositionEstimate> Apply(List<LenientPositionEstimate> items)
    {
        var matched = Collections.Filter(items, (LenientPositionEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient position estimate store.</summary>
public static class LenientPositionEstimateStore
{
    public static ILenientPositionEstimateStore NewMemoryLenientPositionEstimateStore()
    {
        return new MemoryLenientPositionEstimateStore(
            new Dictionary<string, LenientPositionEstimate>(),
            new List<string>());
    }

    public static ILenientPositionEstimateStore NewCachedLenientPositionEstimateStore(MemoryLenientPositionEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedLenientPositionEstimateStore(
            inner,
            LenientPositionEstimateCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<LenientPositionEstimate> FindLenientPositionEstimates(ILenientPositionEstimateStore store, LenientPositionEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
