namespace Acme.Workforce.Compliance.Audit.AuditRecord;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict audit records by code.</summary>
public interface IStrictAuditRecordStore
{
    StrictAuditRecord? Find(string code);
    void Save(StrictAuditRecord item);
    bool Remove(string code);
    List<StrictAuditRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictAuditRecordStore(Dictionary<string, StrictAuditRecord> items, List<string> order) : IStrictAuditRecordStore
{
    public Dictionary<string, StrictAuditRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictAuditRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictAuditRecord item)
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

    public List<StrictAuditRecord> List()
    {
        var outValue = new List<StrictAuditRecord>();
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
public sealed class CachedStrictAuditRecordStore(MemoryStrictAuditRecordStore inner, StrictAuditRecordCache cache, int hits, int misses) : IStrictAuditRecordStore
{
    public MemoryStrictAuditRecordStore Inner { get; set; } = inner;
    public StrictAuditRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictAuditRecord? Find(string code)
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

    public void Save(StrictAuditRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictAuditRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict audit records. Empty fields match all.</summary>
public sealed record StrictAuditRecordQuery(string Title, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(StrictAuditRecord item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictAuditRecord> Apply(List<StrictAuditRecord> items)
    {
        var matched = Collections.Filter(items, (StrictAuditRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict audit record store.</summary>
public static class StrictAuditRecordStore
{
    public static IStrictAuditRecordStore NewMemoryStrictAuditRecordStore()
    {
        return new MemoryStrictAuditRecordStore(new Dictionary<string, StrictAuditRecord>(), new List<string>());
    }

    public static IStrictAuditRecordStore NewCachedStrictAuditRecordStore(MemoryStrictAuditRecordStore inner, int ttl, IClock clock)
    {
        return new CachedStrictAuditRecordStore(inner, StrictAuditRecordCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<StrictAuditRecord> FindStrictAuditRecords(IStrictAuditRecordStore store, StrictAuditRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
