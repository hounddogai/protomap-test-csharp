namespace Acme.Workforce.Workplace.Visitor.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global visitor adjustments by code.</summary>
public interface IGlobalVisitorAdjustmentStore
{
    GlobalVisitorAdjustment? Find(string code);
    void Save(GlobalVisitorAdjustment item);
    bool Remove(string code);
    List<GlobalVisitorAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalVisitorAdjustmentStore(Dictionary<string, GlobalVisitorAdjustment> items, List<string> order) : IGlobalVisitorAdjustmentStore
{
    public Dictionary<string, GlobalVisitorAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalVisitorAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalVisitorAdjustment item)
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

    public List<GlobalVisitorAdjustment> List()
    {
        var outValue = new List<GlobalVisitorAdjustment>();
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
public sealed class CachedGlobalVisitorAdjustmentStore(MemoryGlobalVisitorAdjustmentStore inner, GlobalVisitorAdjustmentCache cache, int hits, int misses) : IGlobalVisitorAdjustmentStore
{
    public MemoryGlobalVisitorAdjustmentStore Inner { get; set; } = inner;
    public GlobalVisitorAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalVisitorAdjustment? Find(string code)
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

    public void Save(GlobalVisitorAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalVisitorAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global visitor adjustments. Empty fields match all.</summary>
public sealed record GlobalVisitorAdjustmentQuery(string Label, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalVisitorAdjustment item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalVisitorAdjustment> Apply(List<GlobalVisitorAdjustment> items)
    {
        var matched = Collections.Filter(items, (GlobalVisitorAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global visitor adjustment store.</summary>
public static class GlobalVisitorAdjustmentStore
{
    public static IGlobalVisitorAdjustmentStore NewMemoryGlobalVisitorAdjustmentStore()
    {
        return new MemoryGlobalVisitorAdjustmentStore(
            new Dictionary<string, GlobalVisitorAdjustment>(),
            new List<string>());
    }

    public static IGlobalVisitorAdjustmentStore NewCachedGlobalVisitorAdjustmentStore(MemoryGlobalVisitorAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalVisitorAdjustmentStore(
            inner,
            GlobalVisitorAdjustmentCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<GlobalVisitorAdjustment> FindGlobalVisitorAdjustments(IGlobalVisitorAdjustmentStore store, GlobalVisitorAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
