namespace Acme.Workforce.Payrollops.PayRun.RunAudit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled pay run audits by code.</summary>
public interface IScheduledPayRunAuditStore
{
    ScheduledPayRunAudit? Find(string code);
    void Save(ScheduledPayRunAudit item);
    bool Remove(string code);
    List<ScheduledPayRunAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledPayRunAuditStore(Dictionary<string, ScheduledPayRunAudit> items, List<string> order) : IScheduledPayRunAuditStore
{
    public Dictionary<string, ScheduledPayRunAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledPayRunAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledPayRunAudit item)
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

    public List<ScheduledPayRunAudit> List()
    {
        var outValue = new List<ScheduledPayRunAudit>();
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
public sealed class CachedScheduledPayRunAuditStore(MemoryScheduledPayRunAuditStore inner, ScheduledPayRunAuditCache cache, int hits, int misses) : IScheduledPayRunAuditStore
{
    public MemoryScheduledPayRunAuditStore Inner { get; set; } = inner;
    public ScheduledPayRunAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledPayRunAudit? Find(string code)
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

    public void Save(ScheduledPayRunAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledPayRunAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled pay run audits. Empty fields match all.</summary>
public sealed record ScheduledPayRunAuditQuery(string Unit, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledPayRunAudit item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledPayRunAudit> Apply(List<ScheduledPayRunAudit> items)
    {
        var matched = Collections.Filter(items, (ScheduledPayRunAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled pay run audit store.</summary>
public static class ScheduledPayRunAuditStore
{
    public static IScheduledPayRunAuditStore NewMemoryScheduledPayRunAuditStore()
    {
        return new MemoryScheduledPayRunAuditStore(
            new Dictionary<string, ScheduledPayRunAudit>(),
            new List<string>());
    }

    public static IScheduledPayRunAuditStore NewCachedScheduledPayRunAuditStore(MemoryScheduledPayRunAuditStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledPayRunAuditStore(inner, ScheduledPayRunAuditCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<ScheduledPayRunAudit> FindScheduledPayRunAudits(IScheduledPayRunAuditStore store, ScheduledPayRunAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
