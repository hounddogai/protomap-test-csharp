namespace Acme.Workforce.Assets.Refresh.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps refresh audits by code.</summary>
public interface IRefreshAuditStore
{
    RefreshAudit? Find(string code);
    void Save(RefreshAudit item);
    bool Remove(string code);
    List<RefreshAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRefreshAuditStore(Dictionary<string, RefreshAudit> items, List<string> order) : IRefreshAuditStore
{
    public Dictionary<string, RefreshAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RefreshAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RefreshAudit item)
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

    public List<RefreshAudit> List()
    {
        var outValue = new List<RefreshAudit>();
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
public sealed class CachedRefreshAuditStore(MemoryRefreshAuditStore inner, RefreshAuditCache cache, int hits, int misses) : IRefreshAuditStore
{
    public MemoryRefreshAuditStore Inner { get; set; } = inner;
    public RefreshAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RefreshAudit? Find(string code)
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

    public void Save(RefreshAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RefreshAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects refresh audits. Empty fields match all.</summary>
public sealed record RefreshAuditQuery(string Title, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(RefreshAudit item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RefreshAudit> Apply(List<RefreshAudit> items)
    {
        var matched = Collections.Filter(items, (RefreshAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of refresh audit store.</summary>
public static class RefreshAuditStore
{
    public static IRefreshAuditStore NewMemoryRefreshAuditStore()
    {
        return new MemoryRefreshAuditStore(new Dictionary<string, RefreshAudit>(), new List<string>());
    }

    public static IRefreshAuditStore NewCachedRefreshAuditStore(MemoryRefreshAuditStore inner, int ttl, IClock clock)
    {
        return new CachedRefreshAuditStore(inner, RefreshAuditCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<RefreshAudit> FindRefreshAudits(IRefreshAuditStore store, RefreshAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
