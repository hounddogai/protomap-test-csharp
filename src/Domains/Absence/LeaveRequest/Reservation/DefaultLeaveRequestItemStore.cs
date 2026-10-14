namespace Acme.Workforce.Absence.LeaveRequest.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default leave request items by code.</summary>
public interface IDefaultLeaveRequestItemStore
{
    DefaultLeaveRequestItem? Find(string code);
    void Save(DefaultLeaveRequestItem item);
    bool Remove(string code);
    List<DefaultLeaveRequestItem> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultLeaveRequestItemStore(Dictionary<string, DefaultLeaveRequestItem> items, List<string> order) : IDefaultLeaveRequestItemStore
{
    public Dictionary<string, DefaultLeaveRequestItem> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultLeaveRequestItem? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultLeaveRequestItem item)
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

    public List<DefaultLeaveRequestItem> List()
    {
        var outValue = new List<DefaultLeaveRequestItem>();
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
public sealed class CachedDefaultLeaveRequestItemStore(MemoryDefaultLeaveRequestItemStore inner, DefaultLeaveRequestItemCache cache, int hits, int misses) : IDefaultLeaveRequestItemStore
{
    public MemoryDefaultLeaveRequestItemStore Inner { get; set; } = inner;
    public DefaultLeaveRequestItemCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultLeaveRequestItem? Find(string code)
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

    public void Save(DefaultLeaveRequestItem item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultLeaveRequestItem> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default leave request items. Empty fields match all.</summary>
public sealed record DefaultLeaveRequestItemQuery(string Country, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultLeaveRequestItem item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
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

    public List<DefaultLeaveRequestItem> Apply(List<DefaultLeaveRequestItem> items)
    {
        var matched = Collections.Filter(items, (DefaultLeaveRequestItem candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default leave request item store.</summary>
public static class DefaultLeaveRequestItemStore
{
    public static IDefaultLeaveRequestItemStore NewMemoryDefaultLeaveRequestItemStore()
    {
        return new MemoryDefaultLeaveRequestItemStore(
            new Dictionary<string, DefaultLeaveRequestItem>(),
            new List<string>());
    }

    public static IDefaultLeaveRequestItemStore NewCachedDefaultLeaveRequestItemStore(MemoryDefaultLeaveRequestItemStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultLeaveRequestItemStore(
            inner,
            DefaultLeaveRequestItemCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<DefaultLeaveRequestItem> FindDefaultLeaveRequestItems(IDefaultLeaveRequestItemStore store, DefaultLeaveRequestItemQuery query)
    {
        return query.Apply(store.List());
    }
}
