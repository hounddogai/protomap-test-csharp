namespace Acme.Workforce.Compensation.EquityGrant.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps equity grant audits by code.</summary>
public interface IEquityGrantAuditStore
{
    EquityGrantAudit? Find(string code);
    void Save(EquityGrantAudit item);
    bool Remove(string code);
    List<EquityGrantAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryEquityGrantAuditStore(Dictionary<string, EquityGrantAudit> items, List<string> order) : IEquityGrantAuditStore
{
    public Dictionary<string, EquityGrantAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public EquityGrantAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(EquityGrantAudit item)
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

    public List<EquityGrantAudit> List()
    {
        var outValue = new List<EquityGrantAudit>();
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
public sealed class CachedEquityGrantAuditStore(MemoryEquityGrantAuditStore inner, EquityGrantAuditCache cache, int hits, int misses) : IEquityGrantAuditStore
{
    public MemoryEquityGrantAuditStore Inner { get; set; } = inner;
    public EquityGrantAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public EquityGrantAudit? Find(string code)
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

    public void Save(EquityGrantAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<EquityGrantAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects equity grant audits. Empty fields match all.</summary>
public sealed record EquityGrantAuditQuery(string Source, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(EquityGrantAudit item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<EquityGrantAudit> Apply(List<EquityGrantAudit> items)
    {
        var matched = Collections.Filter(items, (EquityGrantAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of equity grant audit store.</summary>
public static class EquityGrantAuditStore
{
    public static IEquityGrantAuditStore NewMemoryEquityGrantAuditStore()
    {
        return new MemoryEquityGrantAuditStore(new Dictionary<string, EquityGrantAudit>(), new List<string>());
    }

    public static IEquityGrantAuditStore NewCachedEquityGrantAuditStore(MemoryEquityGrantAuditStore inner, int ttl, IClock clock)
    {
        return new CachedEquityGrantAuditStore(inner, EquityGrantAuditCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<EquityGrantAudit> FindEquityGrantAudits(IEquityGrantAuditStore store, EquityGrantAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
