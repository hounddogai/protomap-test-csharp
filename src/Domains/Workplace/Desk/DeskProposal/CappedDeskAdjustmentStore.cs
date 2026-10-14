namespace Acme.Workforce.Workplace.Desk.DeskProposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped desk adjustments by code.</summary>
public interface ICappedDeskAdjustmentStore
{
    CappedDeskAdjustment? Find(string code);
    void Save(CappedDeskAdjustment item);
    bool Remove(string code);
    List<CappedDeskAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedDeskAdjustmentStore(Dictionary<string, CappedDeskAdjustment> items, List<string> order) : ICappedDeskAdjustmentStore
{
    public Dictionary<string, CappedDeskAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedDeskAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedDeskAdjustment item)
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

    public List<CappedDeskAdjustment> List()
    {
        var outValue = new List<CappedDeskAdjustment>();
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
public sealed class CachedCappedDeskAdjustmentStore(MemoryCappedDeskAdjustmentStore inner, CappedDeskAdjustmentCache cache, int hits, int misses) : ICappedDeskAdjustmentStore
{
    public MemoryCappedDeskAdjustmentStore Inner { get; set; } = inner;
    public CappedDeskAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedDeskAdjustment? Find(string code)
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

    public void Save(CappedDeskAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedDeskAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped desk adjustments. Empty fields match all.</summary>
public sealed record CappedDeskAdjustmentQuery(string Category, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(CappedDeskAdjustment item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedDeskAdjustment> Apply(List<CappedDeskAdjustment> items)
    {
        var matched = Collections.Filter(items, (CappedDeskAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped desk adjustment store.</summary>
public static class CappedDeskAdjustmentStore
{
    public static ICappedDeskAdjustmentStore NewMemoryCappedDeskAdjustmentStore()
    {
        return new MemoryCappedDeskAdjustmentStore(
            new Dictionary<string, CappedDeskAdjustment>(),
            new List<string>());
    }

    public static ICappedDeskAdjustmentStore NewCachedCappedDeskAdjustmentStore(MemoryCappedDeskAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedCappedDeskAdjustmentStore(inner, CappedDeskAdjustmentCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<CappedDeskAdjustment> FindCappedDeskAdjustments(ICappedDeskAdjustmentStore store, CappedDeskAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
