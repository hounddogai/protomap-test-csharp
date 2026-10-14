namespace Acme.Workforce.Compliance.Audit.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional audit records by code.</summary>
public interface IRegionalAuditRecordStore
{
    RegionalAuditRecord? Find(string code);
    void Save(RegionalAuditRecord item);
    bool Remove(string code);
    List<RegionalAuditRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalAuditRecordStore(Dictionary<string, RegionalAuditRecord> items, List<string> order) : IRegionalAuditRecordStore
{
    public Dictionary<string, RegionalAuditRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalAuditRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalAuditRecord item)
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

    public List<RegionalAuditRecord> List()
    {
        var outValue = new List<RegionalAuditRecord>();
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
public sealed class CachedRegionalAuditRecordStore(MemoryRegionalAuditRecordStore inner, RegionalAuditRecordCache cache, int hits, int misses) : IRegionalAuditRecordStore
{
    public MemoryRegionalAuditRecordStore Inner { get; set; } = inner;
    public RegionalAuditRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalAuditRecord? Find(string code)
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

    public void Save(RegionalAuditRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalAuditRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional audit records. Empty fields match all.</summary>
public sealed record RegionalAuditRecordQuery(string Department, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalAuditRecord item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalAuditRecord> Apply(List<RegionalAuditRecord> items)
    {
        var matched = Collections.Filter(items, (RegionalAuditRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional audit record store.</summary>
public static class RegionalAuditRecordStore
{
    public static IRegionalAuditRecordStore NewMemoryRegionalAuditRecordStore()
    {
        return new MemoryRegionalAuditRecordStore(new Dictionary<string, RegionalAuditRecord>(), new List<string>());
    }

    public static IRegionalAuditRecordStore NewCachedRegionalAuditRecordStore(MemoryRegionalAuditRecordStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalAuditRecordStore(inner, RegionalAuditRecordCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<RegionalAuditRecord> FindRegionalAuditRecords(IRegionalAuditRecordStore store, RegionalAuditRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
