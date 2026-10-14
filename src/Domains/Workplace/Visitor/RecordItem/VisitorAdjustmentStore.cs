namespace Acme.Workforce.Workplace.Visitor.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps visitor adjustments by code.</summary>
public interface IVisitorAdjustmentStore
{
    VisitorAdjustment? Find(string code);
    void Save(VisitorAdjustment item);
    bool Remove(string code);
    List<VisitorAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVisitorAdjustmentStore(Dictionary<string, VisitorAdjustment> items, List<string> order) : IVisitorAdjustmentStore
{
    public Dictionary<string, VisitorAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VisitorAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VisitorAdjustment item)
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

    public List<VisitorAdjustment> List()
    {
        var outValue = new List<VisitorAdjustment>();
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
public sealed class CachedVisitorAdjustmentStore(MemoryVisitorAdjustmentStore inner, VisitorAdjustmentCache cache, int hits, int misses) : IVisitorAdjustmentStore
{
    public MemoryVisitorAdjustmentStore Inner { get; set; } = inner;
    public VisitorAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VisitorAdjustment? Find(string code)
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

    public void Save(VisitorAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VisitorAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects visitor adjustments. Empty fields match all.</summary>
public sealed record VisitorAdjustmentQuery(string Region, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(VisitorAdjustment item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VisitorAdjustment> Apply(List<VisitorAdjustment> items)
    {
        var matched = Collections.Filter(items, (VisitorAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of visitor adjustment store.</summary>
public static class VisitorAdjustmentStore
{
    public static IVisitorAdjustmentStore NewMemoryVisitorAdjustmentStore()
    {
        return new MemoryVisitorAdjustmentStore(new Dictionary<string, VisitorAdjustment>(), new List<string>());
    }

    public static IVisitorAdjustmentStore NewCachedVisitorAdjustmentStore(MemoryVisitorAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedVisitorAdjustmentStore(inner, VisitorAdjustmentCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<VisitorAdjustment> FindVisitorAdjustments(IVisitorAdjustmentStore store, VisitorAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
