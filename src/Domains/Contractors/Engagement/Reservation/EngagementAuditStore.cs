namespace Acme.Workforce.Contractors.Engagement.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps engagement audits by code.</summary>
public interface IEngagementAuditStore
{
    EngagementAudit? Find(string code);
    void Save(EngagementAudit item);
    bool Remove(string code);
    List<EngagementAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryEngagementAuditStore(Dictionary<string, EngagementAudit> items, List<string> order) : IEngagementAuditStore
{
    public Dictionary<string, EngagementAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public EngagementAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(EngagementAudit item)
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

    public List<EngagementAudit> List()
    {
        var outValue = new List<EngagementAudit>();
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
public sealed class CachedEngagementAuditStore(MemoryEngagementAuditStore inner, EngagementAuditCache cache, int hits, int misses) : IEngagementAuditStore
{
    public MemoryEngagementAuditStore Inner { get; set; } = inner;
    public EngagementAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public EngagementAudit? Find(string code)
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

    public void Save(EngagementAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<EngagementAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects engagement audits. Empty fields match all.</summary>
public sealed record EngagementAuditQuery(string Name, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(EngagementAudit item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<EngagementAudit> Apply(List<EngagementAudit> items)
    {
        var matched = Collections.Filter(items, (EngagementAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of engagement audit store.</summary>
public static class EngagementAuditStore
{
    public static IEngagementAuditStore NewMemoryEngagementAuditStore()
    {
        return new MemoryEngagementAuditStore(new Dictionary<string, EngagementAudit>(), new List<string>());
    }

    public static IEngagementAuditStore NewCachedEngagementAuditStore(MemoryEngagementAuditStore inner, int ttl, IClock clock)
    {
        return new CachedEngagementAuditStore(inner, EngagementAuditCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<EngagementAudit> FindEngagementAudits(IEngagementAuditStore store, EngagementAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
