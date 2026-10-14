namespace Acme.Workforce.Contractors.StatementOfWork.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default statement of work audits by code.</summary>
public interface IDefaultStatementOfWorkAuditStore
{
    DefaultStatementOfWorkAudit? Find(string code);
    void Save(DefaultStatementOfWorkAudit item);
    bool Remove(string code);
    List<DefaultStatementOfWorkAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultStatementOfWorkAuditStore(Dictionary<string, DefaultStatementOfWorkAudit> items, List<string> order) : IDefaultStatementOfWorkAuditStore
{
    public Dictionary<string, DefaultStatementOfWorkAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultStatementOfWorkAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultStatementOfWorkAudit item)
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

    public List<DefaultStatementOfWorkAudit> List()
    {
        var outValue = new List<DefaultStatementOfWorkAudit>();
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
public sealed class CachedDefaultStatementOfWorkAuditStore(MemoryDefaultStatementOfWorkAuditStore inner, DefaultStatementOfWorkAuditCache cache, int hits, int misses) : IDefaultStatementOfWorkAuditStore
{
    public MemoryDefaultStatementOfWorkAuditStore Inner { get; set; } = inner;
    public DefaultStatementOfWorkAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultStatementOfWorkAudit? Find(string code)
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

    public void Save(DefaultStatementOfWorkAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultStatementOfWorkAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default statement of work audits. Empty fields match all.</summary>
public sealed record DefaultStatementOfWorkAuditQuery(string Region, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultStatementOfWorkAudit item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultStatementOfWorkAudit> Apply(List<DefaultStatementOfWorkAudit> items)
    {
        var matched = Collections.Filter(items, (DefaultStatementOfWorkAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default statement of work audit store.</summary>
public static class DefaultStatementOfWorkAuditStore
{
    public static IDefaultStatementOfWorkAuditStore NewMemoryDefaultStatementOfWorkAuditStore()
    {
        return new MemoryDefaultStatementOfWorkAuditStore(
            new Dictionary<string, DefaultStatementOfWorkAudit>(),
            new List<string>());
    }

    public static IDefaultStatementOfWorkAuditStore NewCachedDefaultStatementOfWorkAuditStore(MemoryDefaultStatementOfWorkAuditStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultStatementOfWorkAuditStore(
            inner,
            DefaultStatementOfWorkAuditCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<DefaultStatementOfWorkAudit> FindDefaultStatementOfWorkAudits(IDefaultStatementOfWorkAuditStore store, DefaultStatementOfWorkAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
