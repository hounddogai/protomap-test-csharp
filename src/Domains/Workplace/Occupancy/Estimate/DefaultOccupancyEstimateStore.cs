namespace Acme.Workforce.Workplace.Occupancy.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default occupancy estimates by code.</summary>
public interface IDefaultOccupancyEstimateStore
{
    DefaultOccupancyEstimate? Find(string code);
    void Save(DefaultOccupancyEstimate item);
    bool Remove(string code);
    List<DefaultOccupancyEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultOccupancyEstimateStore(Dictionary<string, DefaultOccupancyEstimate> items, List<string> order) : IDefaultOccupancyEstimateStore
{
    public Dictionary<string, DefaultOccupancyEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultOccupancyEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultOccupancyEstimate item)
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

    public List<DefaultOccupancyEstimate> List()
    {
        var outValue = new List<DefaultOccupancyEstimate>();
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
public sealed class CachedDefaultOccupancyEstimateStore(MemoryDefaultOccupancyEstimateStore inner, DefaultOccupancyEstimateCache cache, int hits, int misses) : IDefaultOccupancyEstimateStore
{
    public MemoryDefaultOccupancyEstimateStore Inner { get; set; } = inner;
    public DefaultOccupancyEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultOccupancyEstimate? Find(string code)
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

    public void Save(DefaultOccupancyEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultOccupancyEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default occupancy estimates. Empty fields match all.</summary>
public sealed record DefaultOccupancyEstimateQuery(string Label, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultOccupancyEstimate item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<DefaultOccupancyEstimate> Apply(List<DefaultOccupancyEstimate> items)
    {
        var matched = Collections.Filter(items, (DefaultOccupancyEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default occupancy estimate store.</summary>
public static class DefaultOccupancyEstimateStore
{
    public static IDefaultOccupancyEstimateStore NewMemoryDefaultOccupancyEstimateStore()
    {
        return new MemoryDefaultOccupancyEstimateStore(
            new Dictionary<string, DefaultOccupancyEstimate>(),
            new List<string>());
    }

    public static IDefaultOccupancyEstimateStore NewCachedDefaultOccupancyEstimateStore(MemoryDefaultOccupancyEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultOccupancyEstimateStore(
            inner,
            DefaultOccupancyEstimateCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<DefaultOccupancyEstimate> FindDefaultOccupancyEstimates(IDefaultOccupancyEstimateStore store, DefaultOccupancyEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
