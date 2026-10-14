namespace Acme.Workforce.Compliance.Audit.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps audit entries by code.</summary>
public interface IAuditEntryStore
{
    AuditEntry? Find(string code);
    void Save(AuditEntry item);
    bool Remove(string code);
    List<AuditEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAuditEntryStore(Dictionary<string, AuditEntry> items, List<string> order) : IAuditEntryStore
{
    public Dictionary<string, AuditEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AuditEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AuditEntry item)
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

    public List<AuditEntry> List()
    {
        var outValue = new List<AuditEntry>();
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
public sealed class CachedAuditEntryStore(MemoryAuditEntryStore inner, AuditEntryCache cache, int hits, int misses) : IAuditEntryStore
{
    public MemoryAuditEntryStore Inner { get; set; } = inner;
    public AuditEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AuditEntry? Find(string code)
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

    public void Save(AuditEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AuditEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects audit entries. Empty fields match all.</summary>
public sealed record AuditEntryQuery(string Label, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(AuditEntry item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AuditEntry> Apply(List<AuditEntry> items)
    {
        var matched = Collections.Filter(items, (AuditEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of audit entry store.</summary>
public static class AuditEntryStore
{
    public static IAuditEntryStore NewMemoryAuditEntryStore()
    {
        return new MemoryAuditEntryStore(new Dictionary<string, AuditEntry>(), new List<string>());
    }

    public static IAuditEntryStore NewCachedAuditEntryStore(MemoryAuditEntryStore inner, int ttl, IClock clock)
    {
        return new CachedAuditEntryStore(inner, AuditEntryCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<AuditEntry> FindAuditEntries(IAuditEntryStore store, AuditEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
