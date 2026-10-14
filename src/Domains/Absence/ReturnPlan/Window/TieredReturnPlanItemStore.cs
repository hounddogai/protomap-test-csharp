namespace Acme.Workforce.Absence.ReturnPlan.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered return plan items by code.</summary>
public interface ITieredReturnPlanItemStore
{
    TieredReturnPlanItem? Find(string code);
    void Save(TieredReturnPlanItem item);
    bool Remove(string code);
    List<TieredReturnPlanItem> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredReturnPlanItemStore(Dictionary<string, TieredReturnPlanItem> items, List<string> order) : ITieredReturnPlanItemStore
{
    public Dictionary<string, TieredReturnPlanItem> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredReturnPlanItem? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredReturnPlanItem item)
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

    public List<TieredReturnPlanItem> List()
    {
        var outValue = new List<TieredReturnPlanItem>();
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
public sealed class CachedTieredReturnPlanItemStore(MemoryTieredReturnPlanItemStore inner, TieredReturnPlanItemCache cache, int hits, int misses) : ITieredReturnPlanItemStore
{
    public MemoryTieredReturnPlanItemStore Inner { get; set; } = inner;
    public TieredReturnPlanItemCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredReturnPlanItem? Find(string code)
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

    public void Save(TieredReturnPlanItem item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredReturnPlanItem> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered return plan items. Empty fields match all.</summary>
public sealed record TieredReturnPlanItemQuery(string Region, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(TieredReturnPlanItem item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredReturnPlanItem> Apply(List<TieredReturnPlanItem> items)
    {
        var matched = Collections.Filter(items, (TieredReturnPlanItem candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered return plan item store.</summary>
public static class TieredReturnPlanItemStore
{
    public static ITieredReturnPlanItemStore NewMemoryTieredReturnPlanItemStore()
    {
        return new MemoryTieredReturnPlanItemStore(
            new Dictionary<string, TieredReturnPlanItem>(),
            new List<string>());
    }

    public static ITieredReturnPlanItemStore NewCachedTieredReturnPlanItemStore(MemoryTieredReturnPlanItemStore inner, int ttl, IClock clock)
    {
        return new CachedTieredReturnPlanItemStore(inner, TieredReturnPlanItemCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<TieredReturnPlanItem> FindTieredReturnPlanItems(ITieredReturnPlanItemStore store, TieredReturnPlanItemQuery query)
    {
        return query.Apply(store.List());
    }
}
