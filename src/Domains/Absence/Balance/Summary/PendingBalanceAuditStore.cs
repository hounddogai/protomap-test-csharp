namespace Acme.Workforce.Absence.Balance.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending balance audits by code.</summary>
public interface IPendingBalanceAuditStore
{
    PendingBalanceAudit? Find(string code);
    void Save(PendingBalanceAudit item);
    bool Remove(string code);
    List<PendingBalanceAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingBalanceAuditStore(Dictionary<string, PendingBalanceAudit> items, List<string> order) : IPendingBalanceAuditStore
{
    public Dictionary<string, PendingBalanceAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingBalanceAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingBalanceAudit item)
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

    public List<PendingBalanceAudit> List()
    {
        var outValue = new List<PendingBalanceAudit>();
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
public sealed class CachedPendingBalanceAuditStore(MemoryPendingBalanceAuditStore inner, PendingBalanceAuditCache cache, int hits, int misses) : IPendingBalanceAuditStore
{
    public MemoryPendingBalanceAuditStore Inner { get; set; } = inner;
    public PendingBalanceAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingBalanceAudit? Find(string code)
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

    public void Save(PendingBalanceAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingBalanceAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending balance audits. Empty fields match all.</summary>
public sealed record PendingBalanceAuditQuery(string Label, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(PendingBalanceAudit item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingBalanceAudit> Apply(List<PendingBalanceAudit> items)
    {
        var matched = Collections.Filter(items, (PendingBalanceAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending balance audit store.</summary>
public static class PendingBalanceAuditStore
{
    public static IPendingBalanceAuditStore NewMemoryPendingBalanceAuditStore()
    {
        return new MemoryPendingBalanceAuditStore(new Dictionary<string, PendingBalanceAudit>(), new List<string>());
    }

    public static IPendingBalanceAuditStore NewCachedPendingBalanceAuditStore(MemoryPendingBalanceAuditStore inner, int ttl, IClock clock)
    {
        return new CachedPendingBalanceAuditStore(inner, PendingBalanceAuditCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PendingBalanceAudit> FindPendingBalanceAudits(IPendingBalanceAuditStore store, PendingBalanceAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
