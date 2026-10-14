namespace Acme.Workforce.Workplace.AccessBadge.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps access badge batches by code.</summary>
public interface IAccessBadgeBatchStore
{
    AccessBadgeBatch? Find(string code);
    void Save(AccessBadgeBatch item);
    bool Remove(string code);
    List<AccessBadgeBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccessBadgeBatchStore(Dictionary<string, AccessBadgeBatch> items, List<string> order) : IAccessBadgeBatchStore
{
    public Dictionary<string, AccessBadgeBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccessBadgeBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccessBadgeBatch item)
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

    public List<AccessBadgeBatch> List()
    {
        var outValue = new List<AccessBadgeBatch>();
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
public sealed class CachedAccessBadgeBatchStore(MemoryAccessBadgeBatchStore inner, AccessBadgeBatchCache cache, int hits, int misses) : IAccessBadgeBatchStore
{
    public MemoryAccessBadgeBatchStore Inner { get; set; } = inner;
    public AccessBadgeBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccessBadgeBatch? Find(string code)
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

    public void Save(AccessBadgeBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccessBadgeBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects access badge batches. Empty fields match all.</summary>
public sealed record AccessBadgeBatchQuery(string Channel, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(AccessBadgeBatch item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AccessBadgeBatch> Apply(List<AccessBadgeBatch> items)
    {
        var matched = Collections.Filter(items, (AccessBadgeBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of access badge batch store.</summary>
public static class AccessBadgeBatchStore
{
    public static IAccessBadgeBatchStore NewMemoryAccessBadgeBatchStore()
    {
        return new MemoryAccessBadgeBatchStore(new Dictionary<string, AccessBadgeBatch>(), new List<string>());
    }

    public static IAccessBadgeBatchStore NewCachedAccessBadgeBatchStore(MemoryAccessBadgeBatchStore inner, int ttl, IClock clock)
    {
        return new CachedAccessBadgeBatchStore(inner, AccessBadgeBatchCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<AccessBadgeBatch> FindAccessBadgeBatches(IAccessBadgeBatchStore store, AccessBadgeBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
