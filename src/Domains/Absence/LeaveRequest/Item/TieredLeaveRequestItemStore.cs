namespace Acme.Workforce.Absence.LeaveRequest.Item;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered leave request items by code.</summary>
public interface ITieredLeaveRequestItemStore
{
    TieredLeaveRequestItem? Find(string code);
    void Save(TieredLeaveRequestItem item);
    bool Remove(string code);
    List<TieredLeaveRequestItem> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredLeaveRequestItemStore(Dictionary<string, TieredLeaveRequestItem> items, List<string> order) : ITieredLeaveRequestItemStore
{
    public Dictionary<string, TieredLeaveRequestItem> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredLeaveRequestItem? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredLeaveRequestItem item)
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

    public List<TieredLeaveRequestItem> List()
    {
        var outValue = new List<TieredLeaveRequestItem>();
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
public sealed class CachedTieredLeaveRequestItemStore(MemoryTieredLeaveRequestItemStore inner, TieredLeaveRequestItemCache cache, int hits, int misses) : ITieredLeaveRequestItemStore
{
    public MemoryTieredLeaveRequestItemStore Inner { get; set; } = inner;
    public TieredLeaveRequestItemCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredLeaveRequestItem? Find(string code)
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

    public void Save(TieredLeaveRequestItem item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredLeaveRequestItem> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered leave request items. Empty fields match all.</summary>
public sealed record TieredLeaveRequestItemQuery(string Team, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(TieredLeaveRequestItem item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredLeaveRequestItem> Apply(List<TieredLeaveRequestItem> items)
    {
        var matched = Collections.Filter(items, (TieredLeaveRequestItem candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered leave request item store.</summary>
public static class TieredLeaveRequestItemStore
{
    public static ITieredLeaveRequestItemStore NewMemoryTieredLeaveRequestItemStore()
    {
        return new MemoryTieredLeaveRequestItemStore(
            new Dictionary<string, TieredLeaveRequestItem>(),
            new List<string>());
    }

    public static ITieredLeaveRequestItemStore NewCachedTieredLeaveRequestItemStore(MemoryTieredLeaveRequestItemStore inner, int ttl, IClock clock)
    {
        return new CachedTieredLeaveRequestItemStore(
            inner,
            TieredLeaveRequestItemCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<TieredLeaveRequestItem> FindTieredLeaveRequestItems(ITieredLeaveRequestItemStore store, TieredLeaveRequestItemQuery query)
    {
        return query.Apply(store.List());
    }
}
