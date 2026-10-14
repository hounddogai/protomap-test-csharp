namespace Acme.Workforce.Absence.LeaveRequest.Claim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary leave request items by code.</summary>
public interface IPrimaryLeaveRequestItemStore
{
    PrimaryLeaveRequestItem? Find(string code);
    void Save(PrimaryLeaveRequestItem item);
    bool Remove(string code);
    List<PrimaryLeaveRequestItem> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryLeaveRequestItemStore(Dictionary<string, PrimaryLeaveRequestItem> items, List<string> order) : IPrimaryLeaveRequestItemStore
{
    public Dictionary<string, PrimaryLeaveRequestItem> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryLeaveRequestItem? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryLeaveRequestItem item)
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

    public List<PrimaryLeaveRequestItem> List()
    {
        var outValue = new List<PrimaryLeaveRequestItem>();
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
public sealed class CachedPrimaryLeaveRequestItemStore(MemoryPrimaryLeaveRequestItemStore inner, PrimaryLeaveRequestItemCache cache, int hits, int misses) : IPrimaryLeaveRequestItemStore
{
    public MemoryPrimaryLeaveRequestItemStore Inner { get; set; } = inner;
    public PrimaryLeaveRequestItemCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryLeaveRequestItem? Find(string code)
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

    public void Save(PrimaryLeaveRequestItem item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryLeaveRequestItem> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary leave request items. Empty fields match all.</summary>
public sealed record PrimaryLeaveRequestItemQuery(string Name, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryLeaveRequestItem item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryLeaveRequestItem> Apply(List<PrimaryLeaveRequestItem> items)
    {
        var matched = Collections.Filter(items, (PrimaryLeaveRequestItem candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary leave request item store.</summary>
public static class PrimaryLeaveRequestItemStore
{
    public static IPrimaryLeaveRequestItemStore NewMemoryPrimaryLeaveRequestItemStore()
    {
        return new MemoryPrimaryLeaveRequestItemStore(
            new Dictionary<string, PrimaryLeaveRequestItem>(),
            new List<string>());
    }

    public static IPrimaryLeaveRequestItemStore NewCachedPrimaryLeaveRequestItemStore(MemoryPrimaryLeaveRequestItemStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryLeaveRequestItemStore(
            inner,
            PrimaryLeaveRequestItemCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryLeaveRequestItem> FindPrimaryLeaveRequestItems(IPrimaryLeaveRequestItemStore store, PrimaryLeaveRequestItemQuery query)
    {
        return query.Apply(store.List());
    }
}
