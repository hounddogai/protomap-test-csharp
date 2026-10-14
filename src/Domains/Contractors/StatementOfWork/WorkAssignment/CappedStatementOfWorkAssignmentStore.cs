namespace Acme.Workforce.Contractors.StatementOfWork.WorkAssignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped statement of work assignments by code.</summary>
public interface ICappedStatementOfWorkAssignmentStore
{
    CappedStatementOfWorkAssignment? Find(string code);
    void Save(CappedStatementOfWorkAssignment item);
    bool Remove(string code);
    List<CappedStatementOfWorkAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedStatementOfWorkAssignmentStore(Dictionary<string, CappedStatementOfWorkAssignment> items, List<string> order) : ICappedStatementOfWorkAssignmentStore
{
    public Dictionary<string, CappedStatementOfWorkAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedStatementOfWorkAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedStatementOfWorkAssignment item)
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

    public List<CappedStatementOfWorkAssignment> List()
    {
        var outValue = new List<CappedStatementOfWorkAssignment>();
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
public sealed class CachedCappedStatementOfWorkAssignmentStore(MemoryCappedStatementOfWorkAssignmentStore inner, CappedStatementOfWorkAssignmentCache cache, int hits, int misses) : ICappedStatementOfWorkAssignmentStore
{
    public MemoryCappedStatementOfWorkAssignmentStore Inner { get; set; } = inner;
    public CappedStatementOfWorkAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedStatementOfWorkAssignment? Find(string code)
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

    public void Save(CappedStatementOfWorkAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedStatementOfWorkAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped statement of work assignments. Empty fields match all.</summary>
public sealed record CappedStatementOfWorkAssignmentQuery(string Team, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(CappedStatementOfWorkAssignment item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<CappedStatementOfWorkAssignment> Apply(List<CappedStatementOfWorkAssignment> items)
    {
        var matched = Collections.Filter(items, (CappedStatementOfWorkAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped statement of work assignment store.</summary>
public static class CappedStatementOfWorkAssignmentStore
{
    public static ICappedStatementOfWorkAssignmentStore NewMemoryCappedStatementOfWorkAssignmentStore()
    {
        return new MemoryCappedStatementOfWorkAssignmentStore(
            new Dictionary<string, CappedStatementOfWorkAssignment>(),
            new List<string>());
    }

    public static ICappedStatementOfWorkAssignmentStore NewCachedCappedStatementOfWorkAssignmentStore(MemoryCappedStatementOfWorkAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedCappedStatementOfWorkAssignmentStore(
            inner,
            CappedStatementOfWorkAssignmentCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<CappedStatementOfWorkAssignment> FindCappedStatementOfWorkAssignments(ICappedStatementOfWorkAssignmentStore store, CappedStatementOfWorkAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
