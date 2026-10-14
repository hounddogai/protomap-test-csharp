namespace Acme.Workforce.Assets.Refresh.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps refresh allocations by code.</summary>
public interface IRefreshAllocationStore
{
    RefreshAllocation? Find(string code);
    void Save(RefreshAllocation item);
    bool Remove(string code);
    List<RefreshAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRefreshAllocationStore(Dictionary<string, RefreshAllocation> items, List<string> order) : IRefreshAllocationStore
{
    public Dictionary<string, RefreshAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RefreshAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RefreshAllocation item)
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

    public List<RefreshAllocation> List()
    {
        var outValue = new List<RefreshAllocation>();
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
public sealed class CachedRefreshAllocationStore(MemoryRefreshAllocationStore inner, RefreshAllocationCache cache, int hits, int misses) : IRefreshAllocationStore
{
    public MemoryRefreshAllocationStore Inner { get; set; } = inner;
    public RefreshAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RefreshAllocation? Find(string code)
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

    public void Save(RefreshAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RefreshAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects refresh allocations. Empty fields match all.</summary>
public sealed record RefreshAllocationQuery(string Name, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(RefreshAllocation item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RefreshAllocation> Apply(List<RefreshAllocation> items)
    {
        var matched = Collections.Filter(items, (RefreshAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of refresh allocation store.</summary>
public static class RefreshAllocationStore
{
    public static IRefreshAllocationStore NewMemoryRefreshAllocationStore()
    {
        return new MemoryRefreshAllocationStore(new Dictionary<string, RefreshAllocation>(), new List<string>());
    }

    public static IRefreshAllocationStore NewCachedRefreshAllocationStore(MemoryRefreshAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedRefreshAllocationStore(inner, RefreshAllocationCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<RefreshAllocation> FindRefreshAllocations(IRefreshAllocationStore store, RefreshAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
