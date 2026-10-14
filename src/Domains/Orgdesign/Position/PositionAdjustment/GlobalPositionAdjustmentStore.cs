namespace Acme.Workforce.Orgdesign.Position.PositionAdjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global position adjustments by code.</summary>
public interface IGlobalPositionAdjustmentStore
{
    GlobalPositionAdjustment? Find(string code);
    void Save(GlobalPositionAdjustment item);
    bool Remove(string code);
    List<GlobalPositionAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalPositionAdjustmentStore(Dictionary<string, GlobalPositionAdjustment> items, List<string> order) : IGlobalPositionAdjustmentStore
{
    public Dictionary<string, GlobalPositionAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalPositionAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalPositionAdjustment item)
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

    public List<GlobalPositionAdjustment> List()
    {
        var outValue = new List<GlobalPositionAdjustment>();
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
public sealed class CachedGlobalPositionAdjustmentStore(MemoryGlobalPositionAdjustmentStore inner, GlobalPositionAdjustmentCache cache, int hits, int misses) : IGlobalPositionAdjustmentStore
{
    public MemoryGlobalPositionAdjustmentStore Inner { get; set; } = inner;
    public GlobalPositionAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalPositionAdjustment? Find(string code)
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

    public void Save(GlobalPositionAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalPositionAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global position adjustments. Empty fields match all.</summary>
public sealed record GlobalPositionAdjustmentQuery(string Segment, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalPositionAdjustment item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalPositionAdjustment> Apply(List<GlobalPositionAdjustment> items)
    {
        var matched = Collections.Filter(items, (GlobalPositionAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global position adjustment store.</summary>
public static class GlobalPositionAdjustmentStore
{
    public static IGlobalPositionAdjustmentStore NewMemoryGlobalPositionAdjustmentStore()
    {
        return new MemoryGlobalPositionAdjustmentStore(
            new Dictionary<string, GlobalPositionAdjustment>(),
            new List<string>());
    }

    public static IGlobalPositionAdjustmentStore NewCachedGlobalPositionAdjustmentStore(MemoryGlobalPositionAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalPositionAdjustmentStore(
            inner,
            GlobalPositionAdjustmentCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<GlobalPositionAdjustment> FindGlobalPositionAdjustments(IGlobalPositionAdjustmentStore store, GlobalPositionAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
