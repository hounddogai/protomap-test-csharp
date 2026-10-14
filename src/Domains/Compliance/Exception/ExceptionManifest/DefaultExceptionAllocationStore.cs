namespace Acme.Workforce.Compliance.Exception.ExceptionManifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default exception allocations by code.</summary>
public interface IDefaultExceptionAllocationStore
{
    DefaultExceptionAllocation? Find(string code);
    void Save(DefaultExceptionAllocation item);
    bool Remove(string code);
    List<DefaultExceptionAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultExceptionAllocationStore(Dictionary<string, DefaultExceptionAllocation> items, List<string> order) : IDefaultExceptionAllocationStore
{
    public Dictionary<string, DefaultExceptionAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultExceptionAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultExceptionAllocation item)
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

    public List<DefaultExceptionAllocation> List()
    {
        var outValue = new List<DefaultExceptionAllocation>();
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
public sealed class CachedDefaultExceptionAllocationStore(MemoryDefaultExceptionAllocationStore inner, DefaultExceptionAllocationCache cache, int hits, int misses) : IDefaultExceptionAllocationStore
{
    public MemoryDefaultExceptionAllocationStore Inner { get; set; } = inner;
    public DefaultExceptionAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultExceptionAllocation? Find(string code)
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

    public void Save(DefaultExceptionAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultExceptionAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default exception allocations. Empty fields match all.</summary>
public sealed record DefaultExceptionAllocationQuery(string Name, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultExceptionAllocation item)
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

    public List<DefaultExceptionAllocation> Apply(List<DefaultExceptionAllocation> items)
    {
        var matched = Collections.Filter(items, (DefaultExceptionAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default exception allocation store.</summary>
public static class DefaultExceptionAllocationStore
{
    public static IDefaultExceptionAllocationStore NewMemoryDefaultExceptionAllocationStore()
    {
        return new MemoryDefaultExceptionAllocationStore(
            new Dictionary<string, DefaultExceptionAllocation>(),
            new List<string>());
    }

    public static IDefaultExceptionAllocationStore NewCachedDefaultExceptionAllocationStore(MemoryDefaultExceptionAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultExceptionAllocationStore(
            inner,
            DefaultExceptionAllocationCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<DefaultExceptionAllocation> FindDefaultExceptionAllocations(IDefaultExceptionAllocationStore store, DefaultExceptionAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
