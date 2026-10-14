namespace Acme.Workforce.Orgdesign.Position.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps position adjustments by code.</summary>
public interface IPositionAdjustmentStore
{
    PositionAdjustment? Find(string code);
    void Save(PositionAdjustment item);
    bool Remove(string code);
    List<PositionAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPositionAdjustmentStore(Dictionary<string, PositionAdjustment> items, List<string> order) : IPositionAdjustmentStore
{
    public Dictionary<string, PositionAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PositionAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PositionAdjustment item)
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

    public List<PositionAdjustment> List()
    {
        var outValue = new List<PositionAdjustment>();
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
public sealed class CachedPositionAdjustmentStore(MemoryPositionAdjustmentStore inner, PositionAdjustmentCache cache, int hits, int misses) : IPositionAdjustmentStore
{
    public MemoryPositionAdjustmentStore Inner { get; set; } = inner;
    public PositionAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PositionAdjustment? Find(string code)
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

    public void Save(PositionAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PositionAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects position adjustments. Empty fields match all.</summary>
public sealed record PositionAdjustmentQuery(string Name, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(PositionAdjustment item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PositionAdjustment> Apply(List<PositionAdjustment> items)
    {
        var matched = Collections.Filter(items, (PositionAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of position adjustment store.</summary>
public static class PositionAdjustmentStore
{
    public static IPositionAdjustmentStore NewMemoryPositionAdjustmentStore()
    {
        return new MemoryPositionAdjustmentStore(new Dictionary<string, PositionAdjustment>(), new List<string>());
    }

    public static IPositionAdjustmentStore NewCachedPositionAdjustmentStore(MemoryPositionAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedPositionAdjustmentStore(inner, PositionAdjustmentCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PositionAdjustment> FindPositionAdjustments(IPositionAdjustmentStore store, PositionAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
