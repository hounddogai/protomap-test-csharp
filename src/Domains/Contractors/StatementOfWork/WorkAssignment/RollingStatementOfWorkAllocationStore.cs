namespace Acme.Workforce.Contractors.StatementOfWork.WorkAssignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling statement of work allocations by code.</summary>
public interface IRollingStatementOfWorkAllocationStore
{
    RollingStatementOfWorkAllocation? Find(string code);
    void Save(RollingStatementOfWorkAllocation item);
    bool Remove(string code);
    List<RollingStatementOfWorkAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingStatementOfWorkAllocationStore(Dictionary<string, RollingStatementOfWorkAllocation> items, List<string> order) : IRollingStatementOfWorkAllocationStore
{
    public Dictionary<string, RollingStatementOfWorkAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingStatementOfWorkAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingStatementOfWorkAllocation item)
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

    public List<RollingStatementOfWorkAllocation> List()
    {
        var outValue = new List<RollingStatementOfWorkAllocation>();
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
public sealed class CachedRollingStatementOfWorkAllocationStore(MemoryRollingStatementOfWorkAllocationStore inner, RollingStatementOfWorkAllocationCache cache, int hits, int misses) : IRollingStatementOfWorkAllocationStore
{
    public MemoryRollingStatementOfWorkAllocationStore Inner { get; set; } = inner;
    public RollingStatementOfWorkAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingStatementOfWorkAllocation? Find(string code)
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

    public void Save(RollingStatementOfWorkAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingStatementOfWorkAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling statement of work allocations. Empty fields match all.</summary>
public sealed record RollingStatementOfWorkAllocationQuery(string Title, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(RollingStatementOfWorkAllocation item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingStatementOfWorkAllocation> Apply(List<RollingStatementOfWorkAllocation> items)
    {
        var matched = Collections.Filter(items, (RollingStatementOfWorkAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling statement of work allocation store.</summary>
public static class RollingStatementOfWorkAllocationStore
{
    public static IRollingStatementOfWorkAllocationStore NewMemoryRollingStatementOfWorkAllocationStore()
    {
        return new MemoryRollingStatementOfWorkAllocationStore(
            new Dictionary<string, RollingStatementOfWorkAllocation>(),
            new List<string>());
    }

    public static IRollingStatementOfWorkAllocationStore NewCachedRollingStatementOfWorkAllocationStore(MemoryRollingStatementOfWorkAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedRollingStatementOfWorkAllocationStore(
            inner,
            RollingStatementOfWorkAllocationCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<RollingStatementOfWorkAllocation> FindRollingStatementOfWorkAllocations(IRollingStatementOfWorkAllocationStore store, RollingStatementOfWorkAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
