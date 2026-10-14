namespace Acme.Workforce.Workplace.AccessBadge.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling access badge batches by code.</summary>
public interface IRollingAccessBadgeBatchStore
{
    RollingAccessBadgeBatch? Find(string code);
    void Save(RollingAccessBadgeBatch item);
    bool Remove(string code);
    List<RollingAccessBadgeBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingAccessBadgeBatchStore(Dictionary<string, RollingAccessBadgeBatch> items, List<string> order) : IRollingAccessBadgeBatchStore
{
    public Dictionary<string, RollingAccessBadgeBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingAccessBadgeBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingAccessBadgeBatch item)
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

    public List<RollingAccessBadgeBatch> List()
    {
        var outValue = new List<RollingAccessBadgeBatch>();
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
public sealed class CachedRollingAccessBadgeBatchStore(MemoryRollingAccessBadgeBatchStore inner, RollingAccessBadgeBatchCache cache, int hits, int misses) : IRollingAccessBadgeBatchStore
{
    public MemoryRollingAccessBadgeBatchStore Inner { get; set; } = inner;
    public RollingAccessBadgeBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingAccessBadgeBatch? Find(string code)
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

    public void Save(RollingAccessBadgeBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingAccessBadgeBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling access badge batches. Empty fields match all.</summary>
public sealed record RollingAccessBadgeBatchQuery(string Department, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(RollingAccessBadgeBatch item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<RollingAccessBadgeBatch> Apply(List<RollingAccessBadgeBatch> items)
    {
        var matched = Collections.Filter(items, (RollingAccessBadgeBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling access badge batch store.</summary>
public static class RollingAccessBadgeBatchStore
{
    public static IRollingAccessBadgeBatchStore NewMemoryRollingAccessBadgeBatchStore()
    {
        return new MemoryRollingAccessBadgeBatchStore(
            new Dictionary<string, RollingAccessBadgeBatch>(),
            new List<string>());
    }

    public static IRollingAccessBadgeBatchStore NewCachedRollingAccessBadgeBatchStore(MemoryRollingAccessBadgeBatchStore inner, int ttl, IClock clock)
    {
        return new CachedRollingAccessBadgeBatchStore(
            inner,
            RollingAccessBadgeBatchCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<RollingAccessBadgeBatch> FindRollingAccessBadgeBatches(IRollingAccessBadgeBatchStore store, RollingAccessBadgeBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
