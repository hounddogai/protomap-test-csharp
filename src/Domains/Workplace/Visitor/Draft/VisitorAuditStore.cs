namespace Acme.Workforce.Workplace.Visitor.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps visitor audits by code.</summary>
public interface IVisitorAuditStore
{
    VisitorAudit? Find(string code);
    void Save(VisitorAudit item);
    bool Remove(string code);
    List<VisitorAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVisitorAuditStore(Dictionary<string, VisitorAudit> items, List<string> order) : IVisitorAuditStore
{
    public Dictionary<string, VisitorAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VisitorAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VisitorAudit item)
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

    public List<VisitorAudit> List()
    {
        var outValue = new List<VisitorAudit>();
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
public sealed class CachedVisitorAuditStore(MemoryVisitorAuditStore inner, VisitorAuditCache cache, int hits, int misses) : IVisitorAuditStore
{
    public MemoryVisitorAuditStore Inner { get; set; } = inner;
    public VisitorAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VisitorAudit? Find(string code)
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

    public void Save(VisitorAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VisitorAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects visitor audits. Empty fields match all.</summary>
public sealed record VisitorAuditQuery(string Label, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(VisitorAudit item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VisitorAudit> Apply(List<VisitorAudit> items)
    {
        var matched = Collections.Filter(items, (VisitorAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of visitor audit store.</summary>
public static class VisitorAuditStore
{
    public static IVisitorAuditStore NewMemoryVisitorAuditStore()
    {
        return new MemoryVisitorAuditStore(new Dictionary<string, VisitorAudit>(), new List<string>());
    }

    public static IVisitorAuditStore NewCachedVisitorAuditStore(MemoryVisitorAuditStore inner, int ttl, IClock clock)
    {
        return new CachedVisitorAuditStore(inner, VisitorAuditCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<VisitorAudit> FindVisitorAudits(IVisitorAuditStore store, VisitorAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
