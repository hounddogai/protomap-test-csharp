namespace Acme.Workforce.Assets.Assignment.Claim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps assignment audits by code.</summary>
public interface IAssignmentAuditStore
{
    AssignmentAudit? Find(string code);
    void Save(AssignmentAudit item);
    bool Remove(string code);
    List<AssignmentAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssignmentAuditStore(Dictionary<string, AssignmentAudit> items, List<string> order) : IAssignmentAuditStore
{
    public Dictionary<string, AssignmentAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssignmentAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssignmentAudit item)
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

    public List<AssignmentAudit> List()
    {
        var outValue = new List<AssignmentAudit>();
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
public sealed class CachedAssignmentAuditStore(MemoryAssignmentAuditStore inner, AssignmentAuditCache cache, int hits, int misses) : IAssignmentAuditStore
{
    public MemoryAssignmentAuditStore Inner { get; set; } = inner;
    public AssignmentAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssignmentAudit? Find(string code)
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

    public void Save(AssignmentAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssignmentAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects assignment audits. Empty fields match all.</summary>
public sealed record AssignmentAuditQuery(string Team, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(AssignmentAudit item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AssignmentAudit> Apply(List<AssignmentAudit> items)
    {
        var matched = Collections.Filter(items, (AssignmentAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of assignment audit store.</summary>
public static class AssignmentAuditStore
{
    public static IAssignmentAuditStore NewMemoryAssignmentAuditStore()
    {
        return new MemoryAssignmentAuditStore(new Dictionary<string, AssignmentAudit>(), new List<string>());
    }

    public static IAssignmentAuditStore NewCachedAssignmentAuditStore(MemoryAssignmentAuditStore inner, int ttl, IClock clock)
    {
        return new CachedAssignmentAuditStore(inner, AssignmentAuditCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<AssignmentAudit> FindAssignmentAudits(IAssignmentAuditStore store, AssignmentAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
