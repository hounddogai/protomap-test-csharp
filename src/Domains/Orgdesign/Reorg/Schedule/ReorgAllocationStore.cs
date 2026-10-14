namespace Acme.Workforce.Orgdesign.Reorg.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps reorg allocations by code.</summary>
public interface IReorgAllocationStore
{
    ReorgAllocation? Find(string code);
    void Save(ReorgAllocation item);
    bool Remove(string code);
    List<ReorgAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReorgAllocationStore(Dictionary<string, ReorgAllocation> items, List<string> order) : IReorgAllocationStore
{
    public Dictionary<string, ReorgAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReorgAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReorgAllocation item)
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

    public List<ReorgAllocation> List()
    {
        var outValue = new List<ReorgAllocation>();
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
public sealed class CachedReorgAllocationStore(MemoryReorgAllocationStore inner, ReorgAllocationCache cache, int hits, int misses) : IReorgAllocationStore
{
    public MemoryReorgAllocationStore Inner { get; set; } = inner;
    public ReorgAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReorgAllocation? Find(string code)
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

    public void Save(ReorgAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReorgAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects reorg allocations. Empty fields match all.</summary>
public sealed record ReorgAllocationQuery(string Name, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(ReorgAllocation item)
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

    public List<ReorgAllocation> Apply(List<ReorgAllocation> items)
    {
        var matched = Collections.Filter(items, (ReorgAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of reorg allocation store.</summary>
public static class ReorgAllocationStore
{
    public static IReorgAllocationStore NewMemoryReorgAllocationStore()
    {
        return new MemoryReorgAllocationStore(new Dictionary<string, ReorgAllocation>(), new List<string>());
    }

    public static IReorgAllocationStore NewCachedReorgAllocationStore(MemoryReorgAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedReorgAllocationStore(inner, ReorgAllocationCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<ReorgAllocation> FindReorgAllocations(IReorgAllocationStore store, ReorgAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
