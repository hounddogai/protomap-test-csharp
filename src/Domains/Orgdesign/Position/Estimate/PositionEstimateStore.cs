namespace Acme.Workforce.Orgdesign.Position.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps position estimates by code.</summary>
public interface IPositionEstimateStore
{
    PositionEstimate? Find(string code);
    void Save(PositionEstimate item);
    bool Remove(string code);
    List<PositionEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPositionEstimateStore(Dictionary<string, PositionEstimate> items, List<string> order) : IPositionEstimateStore
{
    public Dictionary<string, PositionEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PositionEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PositionEstimate item)
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

    public List<PositionEstimate> List()
    {
        var outValue = new List<PositionEstimate>();
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
public sealed class CachedPositionEstimateStore(MemoryPositionEstimateStore inner, PositionEstimateCache cache, int hits, int misses) : IPositionEstimateStore
{
    public MemoryPositionEstimateStore Inner { get; set; } = inner;
    public PositionEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PositionEstimate? Find(string code)
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

    public void Save(PositionEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PositionEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects position estimates. Empty fields match all.</summary>
public sealed record PositionEstimateQuery(string Name, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(PositionEstimate item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<PositionEstimate> Apply(List<PositionEstimate> items)
    {
        var matched = Collections.Filter(items, (PositionEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of position estimate store.</summary>
public static class PositionEstimateStore
{
    public static IPositionEstimateStore NewMemoryPositionEstimateStore()
    {
        return new MemoryPositionEstimateStore(new Dictionary<string, PositionEstimate>(), new List<string>());
    }

    public static IPositionEstimateStore NewCachedPositionEstimateStore(MemoryPositionEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedPositionEstimateStore(inner, PositionEstimateCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PositionEstimate> FindPositionEstimates(IPositionEstimateStore store, PositionEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
