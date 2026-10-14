namespace Acme.Workforce.Compliance.Audit.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps audit mappings by code.</summary>
public interface IAuditMappingStore
{
    AuditMapping? Find(string code);
    void Save(AuditMapping item);
    bool Remove(string code);
    List<AuditMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAuditMappingStore(Dictionary<string, AuditMapping> items, List<string> order) : IAuditMappingStore
{
    public Dictionary<string, AuditMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AuditMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AuditMapping item)
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

    public List<AuditMapping> List()
    {
        var outValue = new List<AuditMapping>();
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
public sealed class CachedAuditMappingStore(MemoryAuditMappingStore inner, AuditMappingCache cache, int hits, int misses) : IAuditMappingStore
{
    public MemoryAuditMappingStore Inner { get; set; } = inner;
    public AuditMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AuditMapping? Find(string code)
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

    public void Save(AuditMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AuditMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects audit mappings. Empty fields match all.</summary>
public sealed record AuditMappingQuery(string Segment, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(AuditMapping item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AuditMapping> Apply(List<AuditMapping> items)
    {
        var matched = Collections.Filter(items, (AuditMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of audit mapping store.</summary>
public static class AuditMappingStore
{
    public static IAuditMappingStore NewMemoryAuditMappingStore()
    {
        return new MemoryAuditMappingStore(new Dictionary<string, AuditMapping>(), new List<string>());
    }

    public static IAuditMappingStore NewCachedAuditMappingStore(MemoryAuditMappingStore inner, int ttl, IClock clock)
    {
        return new CachedAuditMappingStore(inner, AuditMappingCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<AuditMapping> FindAuditMappings(IAuditMappingStore store, AuditMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
