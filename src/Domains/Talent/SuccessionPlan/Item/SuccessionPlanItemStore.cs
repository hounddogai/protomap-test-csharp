namespace Acme.Workforce.Talent.SuccessionPlan.Item;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps succession plan items by code.</summary>
public interface ISuccessionPlanItemStore
{
    SuccessionPlanItem? Find(string code);
    void Save(SuccessionPlanItem item);
    bool Remove(string code);
    List<SuccessionPlanItem> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySuccessionPlanItemStore(Dictionary<string, SuccessionPlanItem> items, List<string> order) : ISuccessionPlanItemStore
{
    public Dictionary<string, SuccessionPlanItem> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SuccessionPlanItem? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SuccessionPlanItem item)
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

    public List<SuccessionPlanItem> List()
    {
        var outValue = new List<SuccessionPlanItem>();
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
public sealed class CachedSuccessionPlanItemStore(MemorySuccessionPlanItemStore inner, SuccessionPlanItemCache cache, int hits, int misses) : ISuccessionPlanItemStore
{
    public MemorySuccessionPlanItemStore Inner { get; set; } = inner;
    public SuccessionPlanItemCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SuccessionPlanItem? Find(string code)
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

    public void Save(SuccessionPlanItem item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SuccessionPlanItem> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects succession plan items. Empty fields match all.</summary>
public sealed record SuccessionPlanItemQuery(string Source, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(SuccessionPlanItem item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SuccessionPlanItem> Apply(List<SuccessionPlanItem> items)
    {
        var matched = Collections.Filter(items, (SuccessionPlanItem candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of succession plan item store.</summary>
public static class SuccessionPlanItemStore
{
    public static ISuccessionPlanItemStore NewMemorySuccessionPlanItemStore()
    {
        return new MemorySuccessionPlanItemStore(new Dictionary<string, SuccessionPlanItem>(), new List<string>());
    }

    public static ISuccessionPlanItemStore NewCachedSuccessionPlanItemStore(MemorySuccessionPlanItemStore inner, int ttl, IClock clock)
    {
        return new CachedSuccessionPlanItemStore(inner, SuccessionPlanItemCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<SuccessionPlanItem> FindSuccessionPlanItems(ISuccessionPlanItemStore store, SuccessionPlanItemQuery query)
    {
        return query.Apply(store.List());
    }
}
