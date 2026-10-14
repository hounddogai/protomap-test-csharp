namespace Acme.Workforce.Compliance.Audit.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending audit records by code.</summary>
public interface IPendingAuditRecordStore
{
    PendingAuditRecord? Find(string code);
    void Save(PendingAuditRecord item);
    bool Remove(string code);
    List<PendingAuditRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingAuditRecordStore(Dictionary<string, PendingAuditRecord> items, List<string> order) : IPendingAuditRecordStore
{
    public Dictionary<string, PendingAuditRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingAuditRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingAuditRecord item)
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

    public List<PendingAuditRecord> List()
    {
        var outValue = new List<PendingAuditRecord>();
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
public sealed class CachedPendingAuditRecordStore(MemoryPendingAuditRecordStore inner, PendingAuditRecordCache cache, int hits, int misses) : IPendingAuditRecordStore
{
    public MemoryPendingAuditRecordStore Inner { get; set; } = inner;
    public PendingAuditRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingAuditRecord? Find(string code)
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

    public void Save(PendingAuditRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingAuditRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending audit records. Empty fields match all.</summary>
public sealed record PendingAuditRecordQuery(string Title, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(PendingAuditRecord item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingAuditRecord> Apply(List<PendingAuditRecord> items)
    {
        var matched = Collections.Filter(items, (PendingAuditRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending audit record store.</summary>
public static class PendingAuditRecordStore
{
    public static IPendingAuditRecordStore NewMemoryPendingAuditRecordStore()
    {
        return new MemoryPendingAuditRecordStore(new Dictionary<string, PendingAuditRecord>(), new List<string>());
    }

    public static IPendingAuditRecordStore NewCachedPendingAuditRecordStore(MemoryPendingAuditRecordStore inner, int ttl, IClock clock)
    {
        return new CachedPendingAuditRecordStore(inner, PendingAuditRecordCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PendingAuditRecord> FindPendingAuditRecords(IPendingAuditRecordStore store, PendingAuditRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
