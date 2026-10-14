namespace Acme.Workforce.Workplace.Desk.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps desk adjustments by code.</summary>
public interface IDeskAdjustmentStore
{
    DeskAdjustment? Find(string code);
    void Save(DeskAdjustment item);
    bool Remove(string code);
    List<DeskAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeskAdjustmentStore(Dictionary<string, DeskAdjustment> items, List<string> order) : IDeskAdjustmentStore
{
    public Dictionary<string, DeskAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeskAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeskAdjustment item)
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

    public List<DeskAdjustment> List()
    {
        var outValue = new List<DeskAdjustment>();
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
public sealed class CachedDeskAdjustmentStore(MemoryDeskAdjustmentStore inner, DeskAdjustmentCache cache, int hits, int misses) : IDeskAdjustmentStore
{
    public MemoryDeskAdjustmentStore Inner { get; set; } = inner;
    public DeskAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeskAdjustment? Find(string code)
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

    public void Save(DeskAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeskAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects desk adjustments. Empty fields match all.</summary>
public sealed record DeskAdjustmentQuery(string Locale, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(DeskAdjustment item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
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

    public List<DeskAdjustment> Apply(List<DeskAdjustment> items)
    {
        var matched = Collections.Filter(items, (DeskAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of desk adjustment store.</summary>
public static class DeskAdjustmentStore
{
    public static IDeskAdjustmentStore NewMemoryDeskAdjustmentStore()
    {
        return new MemoryDeskAdjustmentStore(new Dictionary<string, DeskAdjustment>(), new List<string>());
    }

    public static IDeskAdjustmentStore NewCachedDeskAdjustmentStore(MemoryDeskAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedDeskAdjustmentStore(inner, DeskAdjustmentCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<DeskAdjustment> FindDeskAdjustments(IDeskAdjustmentStore store, DeskAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
