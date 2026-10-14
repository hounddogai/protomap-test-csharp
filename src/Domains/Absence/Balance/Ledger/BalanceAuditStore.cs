namespace Acme.Workforce.Absence.Balance.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps balance audits by code.</summary>
public interface IBalanceAuditStore
{
    BalanceAudit? Find(string code);
    void Save(BalanceAudit item);
    bool Remove(string code);
    List<BalanceAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBalanceAuditStore(Dictionary<string, BalanceAudit> items, List<string> order) : IBalanceAuditStore
{
    public Dictionary<string, BalanceAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BalanceAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BalanceAudit item)
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

    public List<BalanceAudit> List()
    {
        var outValue = new List<BalanceAudit>();
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
public sealed class CachedBalanceAuditStore(MemoryBalanceAuditStore inner, BalanceAuditCache cache, int hits, int misses) : IBalanceAuditStore
{
    public MemoryBalanceAuditStore Inner { get; set; } = inner;
    public BalanceAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BalanceAudit? Find(string code)
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

    public void Save(BalanceAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BalanceAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects balance audits. Empty fields match all.</summary>
public sealed record BalanceAuditQuery(string Title, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(BalanceAudit item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BalanceAudit> Apply(List<BalanceAudit> items)
    {
        var matched = Collections.Filter(items, (BalanceAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of balance audit store.</summary>
public static class BalanceAuditStore
{
    public static IBalanceAuditStore NewMemoryBalanceAuditStore()
    {
        return new MemoryBalanceAuditStore(new Dictionary<string, BalanceAudit>(), new List<string>());
    }

    public static IBalanceAuditStore NewCachedBalanceAuditStore(MemoryBalanceAuditStore inner, int ttl, IClock clock)
    {
        return new CachedBalanceAuditStore(inner, BalanceAuditCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<BalanceAudit> FindBalanceAudits(IBalanceAuditStore store, BalanceAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
