namespace Acme.Workforce.Absence.LeaveRequest.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps leave request audits by code.</summary>
public interface ILeaveRequestAuditStore
{
    LeaveRequestAudit? Find(string code);
    void Save(LeaveRequestAudit item);
    bool Remove(string code);
    List<LeaveRequestAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLeaveRequestAuditStore(Dictionary<string, LeaveRequestAudit> items, List<string> order) : ILeaveRequestAuditStore
{
    public Dictionary<string, LeaveRequestAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LeaveRequestAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LeaveRequestAudit item)
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

    public List<LeaveRequestAudit> List()
    {
        var outValue = new List<LeaveRequestAudit>();
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
public sealed class CachedLeaveRequestAuditStore(MemoryLeaveRequestAuditStore inner, LeaveRequestAuditCache cache, int hits, int misses) : ILeaveRequestAuditStore
{
    public MemoryLeaveRequestAuditStore Inner { get; set; } = inner;
    public LeaveRequestAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LeaveRequestAudit? Find(string code)
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

    public void Save(LeaveRequestAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LeaveRequestAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects leave request audits. Empty fields match all.</summary>
public sealed record LeaveRequestAuditQuery(string Team, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(LeaveRequestAudit item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LeaveRequestAudit> Apply(List<LeaveRequestAudit> items)
    {
        var matched = Collections.Filter(items, (LeaveRequestAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of leave request audit store.</summary>
public static class LeaveRequestAuditStore
{
    public static ILeaveRequestAuditStore NewMemoryLeaveRequestAuditStore()
    {
        return new MemoryLeaveRequestAuditStore(new Dictionary<string, LeaveRequestAudit>(), new List<string>());
    }

    public static ILeaveRequestAuditStore NewCachedLeaveRequestAuditStore(MemoryLeaveRequestAuditStore inner, int ttl, IClock clock)
    {
        return new CachedLeaveRequestAuditStore(inner, LeaveRequestAuditCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<LeaveRequestAudit> FindLeaveRequestAudits(ILeaveRequestAuditStore store, LeaveRequestAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
