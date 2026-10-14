namespace Acme.Workforce.Compliance.Audit.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps audit items by code.</summary>
public interface IAuditItemStore
{
    AuditItem? Find(string code);
    void Save(AuditItem item);
    bool Remove(string code);
    List<AuditItem> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAuditItemStore(Dictionary<string, AuditItem> items, List<string> order) : IAuditItemStore
{
    public Dictionary<string, AuditItem> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AuditItem? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AuditItem item)
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

    public List<AuditItem> List()
    {
        var outValue = new List<AuditItem>();
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
public sealed class CachedAuditItemStore(MemoryAuditItemStore inner, AuditItemCache cache, int hits, int misses) : IAuditItemStore
{
    public MemoryAuditItemStore Inner { get; set; } = inner;
    public AuditItemCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AuditItem? Find(string code)
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

    public void Save(AuditItem item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AuditItem> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects audit items. Empty fields match all.</summary>
public sealed record AuditItemQuery(string Label, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(AuditItem item)
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

    public List<AuditItem> Apply(List<AuditItem> items)
    {
        var matched = Collections.Filter(items, (AuditItem candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of audit item store.</summary>
public static class AuditItemStore
{
    public static IAuditItemStore NewMemoryAuditItemStore()
    {
        return new MemoryAuditItemStore(new Dictionary<string, AuditItem>(), new List<string>());
    }

    public static IAuditItemStore NewCachedAuditItemStore(MemoryAuditItemStore inner, int ttl, IClock clock)
    {
        return new CachedAuditItemStore(inner, AuditItemCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<AuditItem> FindAuditItems(IAuditItemStore store, AuditItemQuery query)
    {
        return query.Apply(store.List());
    }
}
