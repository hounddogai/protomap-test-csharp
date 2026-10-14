namespace Acme.Workforce.Contractors.StatementOfWork.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps statement of work assignments by code.</summary>
public interface IStatementOfWorkAssignmentStore
{
    StatementOfWorkAssignment? Find(string code);
    void Save(StatementOfWorkAssignment item);
    bool Remove(string code);
    List<StatementOfWorkAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStatementOfWorkAssignmentStore(Dictionary<string, StatementOfWorkAssignment> items, List<string> order) : IStatementOfWorkAssignmentStore
{
    public Dictionary<string, StatementOfWorkAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StatementOfWorkAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StatementOfWorkAssignment item)
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

    public List<StatementOfWorkAssignment> List()
    {
        var outValue = new List<StatementOfWorkAssignment>();
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
public sealed class CachedStatementOfWorkAssignmentStore(MemoryStatementOfWorkAssignmentStore inner, StatementOfWorkAssignmentCache cache, int hits, int misses) : IStatementOfWorkAssignmentStore
{
    public MemoryStatementOfWorkAssignmentStore Inner { get; set; } = inner;
    public StatementOfWorkAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StatementOfWorkAssignment? Find(string code)
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

    public void Save(StatementOfWorkAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StatementOfWorkAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects statement of work assignments. Empty fields match all.</summary>
public sealed record StatementOfWorkAssignmentQuery(string Name, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(StatementOfWorkAssignment item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<StatementOfWorkAssignment> Apply(List<StatementOfWorkAssignment> items)
    {
        var matched = Collections.Filter(items, (StatementOfWorkAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of statement of work assignment store.</summary>
public static class StatementOfWorkAssignmentStore
{
    public static IStatementOfWorkAssignmentStore NewMemoryStatementOfWorkAssignmentStore()
    {
        return new MemoryStatementOfWorkAssignmentStore(
            new Dictionary<string, StatementOfWorkAssignment>(),
            new List<string>());
    }

    public static IStatementOfWorkAssignmentStore NewCachedStatementOfWorkAssignmentStore(MemoryStatementOfWorkAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedStatementOfWorkAssignmentStore(
            inner,
            StatementOfWorkAssignmentCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<StatementOfWorkAssignment> FindStatementOfWorkAssignments(IStatementOfWorkAssignmentStore store, StatementOfWorkAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
