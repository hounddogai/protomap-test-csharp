namespace Acme.Workforce.Absence.LeaveRequest.RequestClaim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling leave request batches by code.</summary>
public interface IRollingLeaveRequestBatchStore
{
    RollingLeaveRequestBatch? Find(string code);
    void Save(RollingLeaveRequestBatch item);
    bool Remove(string code);
    List<RollingLeaveRequestBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingLeaveRequestBatchStore(Dictionary<string, RollingLeaveRequestBatch> items, List<string> order) : IRollingLeaveRequestBatchStore
{
    public Dictionary<string, RollingLeaveRequestBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingLeaveRequestBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingLeaveRequestBatch item)
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

    public List<RollingLeaveRequestBatch> List()
    {
        var outValue = new List<RollingLeaveRequestBatch>();
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
public sealed class CachedRollingLeaveRequestBatchStore(MemoryRollingLeaveRequestBatchStore inner, RollingLeaveRequestBatchCache cache, int hits, int misses) : IRollingLeaveRequestBatchStore
{
    public MemoryRollingLeaveRequestBatchStore Inner { get; set; } = inner;
    public RollingLeaveRequestBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingLeaveRequestBatch? Find(string code)
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

    public void Save(RollingLeaveRequestBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingLeaveRequestBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling leave request batches. Empty fields match all.</summary>
public sealed record RollingLeaveRequestBatchQuery(string Unit, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(RollingLeaveRequestBatch item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingLeaveRequestBatch> Apply(List<RollingLeaveRequestBatch> items)
    {
        var matched = Collections.Filter(items, (RollingLeaveRequestBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling leave request batch store.</summary>
public static class RollingLeaveRequestBatchStore
{
    public static IRollingLeaveRequestBatchStore NewMemoryRollingLeaveRequestBatchStore()
    {
        return new MemoryRollingLeaveRequestBatchStore(
            new Dictionary<string, RollingLeaveRequestBatch>(),
            new List<string>());
    }

    public static IRollingLeaveRequestBatchStore NewCachedRollingLeaveRequestBatchStore(MemoryRollingLeaveRequestBatchStore inner, int ttl, IClock clock)
    {
        return new CachedRollingLeaveRequestBatchStore(
            inner,
            RollingLeaveRequestBatchCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<RollingLeaveRequestBatch> FindRollingLeaveRequestBatches(IRollingLeaveRequestBatchStore store, RollingLeaveRequestBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
