namespace Acme.Workforce.Compliance.Audit.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps audit requests by code.</summary>
public interface IAuditRequestStore
{
    AuditRequest? Find(string code);
    void Save(AuditRequest item);
    bool Remove(string code);
    List<AuditRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAuditRequestStore(Dictionary<string, AuditRequest> items, List<string> order) : IAuditRequestStore
{
    public Dictionary<string, AuditRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AuditRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AuditRequest item)
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

    public List<AuditRequest> List()
    {
        var outValue = new List<AuditRequest>();
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
public sealed class CachedAuditRequestStore(MemoryAuditRequestStore inner, AuditRequestCache cache, int hits, int misses) : IAuditRequestStore
{
    public MemoryAuditRequestStore Inner { get; set; } = inner;
    public AuditRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AuditRequest? Find(string code)
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

    public void Save(AuditRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AuditRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects audit requests. Empty fields match all.</summary>
public sealed record AuditRequestQuery(string Label, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(AuditRequest item)
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

    public List<AuditRequest> Apply(List<AuditRequest> items)
    {
        var matched = Collections.Filter(items, (AuditRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of audit request store.</summary>
public static class AuditRequestStore
{
    public static IAuditRequestStore NewMemoryAuditRequestStore()
    {
        return new MemoryAuditRequestStore(new Dictionary<string, AuditRequest>(), new List<string>());
    }

    public static IAuditRequestStore NewCachedAuditRequestStore(MemoryAuditRequestStore inner, int ttl, IClock clock)
    {
        return new CachedAuditRequestStore(inner, AuditRequestCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<AuditRequest> FindAuditRequests(IAuditRequestStore store, AuditRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
