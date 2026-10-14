namespace Acme.Workforce.Absence.LeaveRequest.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional leave request items by code.</summary>
public interface IRegionalLeaveRequestItemStore
{
    RegionalLeaveRequestItem? Find(string code);
    void Save(RegionalLeaveRequestItem item);
    bool Remove(string code);
    List<RegionalLeaveRequestItem> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalLeaveRequestItemStore(Dictionary<string, RegionalLeaveRequestItem> items, List<string> order) : IRegionalLeaveRequestItemStore
{
    public Dictionary<string, RegionalLeaveRequestItem> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalLeaveRequestItem? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalLeaveRequestItem item)
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

    public List<RegionalLeaveRequestItem> List()
    {
        var outValue = new List<RegionalLeaveRequestItem>();
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
public sealed class CachedRegionalLeaveRequestItemStore(MemoryRegionalLeaveRequestItemStore inner, RegionalLeaveRequestItemCache cache, int hits, int misses) : IRegionalLeaveRequestItemStore
{
    public MemoryRegionalLeaveRequestItemStore Inner { get; set; } = inner;
    public RegionalLeaveRequestItemCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalLeaveRequestItem? Find(string code)
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

    public void Save(RegionalLeaveRequestItem item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalLeaveRequestItem> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional leave request items. Empty fields match all.</summary>
public sealed record RegionalLeaveRequestItemQuery(string Label, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalLeaveRequestItem item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalLeaveRequestItem> Apply(List<RegionalLeaveRequestItem> items)
    {
        var matched = Collections.Filter(items, (RegionalLeaveRequestItem candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional leave request item store.</summary>
public static class RegionalLeaveRequestItemStore
{
    public static IRegionalLeaveRequestItemStore NewMemoryRegionalLeaveRequestItemStore()
    {
        return new MemoryRegionalLeaveRequestItemStore(
            new Dictionary<string, RegionalLeaveRequestItem>(),
            new List<string>());
    }

    public static IRegionalLeaveRequestItemStore NewCachedRegionalLeaveRequestItemStore(MemoryRegionalLeaveRequestItemStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalLeaveRequestItemStore(
            inner,
            RegionalLeaveRequestItemCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<RegionalLeaveRequestItem> FindRegionalLeaveRequestItems(IRegionalLeaveRequestItemStore store, RegionalLeaveRequestItemQuery query)
    {
        return query.Apply(store.List());
    }
}
