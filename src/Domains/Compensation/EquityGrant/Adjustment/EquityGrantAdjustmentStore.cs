namespace Acme.Workforce.Compensation.EquityGrant.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps equity grant adjustments by code.</summary>
public interface IEquityGrantAdjustmentStore
{
    EquityGrantAdjustment? Find(string code);
    void Save(EquityGrantAdjustment item);
    bool Remove(string code);
    List<EquityGrantAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryEquityGrantAdjustmentStore(Dictionary<string, EquityGrantAdjustment> items, List<string> order) : IEquityGrantAdjustmentStore
{
    public Dictionary<string, EquityGrantAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public EquityGrantAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(EquityGrantAdjustment item)
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

    public List<EquityGrantAdjustment> List()
    {
        var outValue = new List<EquityGrantAdjustment>();
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
public sealed class CachedEquityGrantAdjustmentStore(MemoryEquityGrantAdjustmentStore inner, EquityGrantAdjustmentCache cache, int hits, int misses) : IEquityGrantAdjustmentStore
{
    public MemoryEquityGrantAdjustmentStore Inner { get; set; } = inner;
    public EquityGrantAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public EquityGrantAdjustment? Find(string code)
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

    public void Save(EquityGrantAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<EquityGrantAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects equity grant adjustments. Empty fields match all.</summary>
public sealed record EquityGrantAdjustmentQuery(string Title, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(EquityGrantAdjustment item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<EquityGrantAdjustment> Apply(List<EquityGrantAdjustment> items)
    {
        var matched = Collections.Filter(items, (EquityGrantAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of equity grant adjustment store.</summary>
public static class EquityGrantAdjustmentStore
{
    public static IEquityGrantAdjustmentStore NewMemoryEquityGrantAdjustmentStore()
    {
        return new MemoryEquityGrantAdjustmentStore(
            new Dictionary<string, EquityGrantAdjustment>(),
            new List<string>());
    }

    public static IEquityGrantAdjustmentStore NewCachedEquityGrantAdjustmentStore(MemoryEquityGrantAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedEquityGrantAdjustmentStore(inner, EquityGrantAdjustmentCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<EquityGrantAdjustment> FindEquityGrantAdjustments(IEquityGrantAdjustmentStore store, EquityGrantAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
