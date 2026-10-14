namespace Acme.Workforce.Compliance.Audit.AuditBaseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global audit mappings by code.</summary>
public interface IGlobalAuditMappingStore
{
    GlobalAuditMapping? Find(string code);
    void Save(GlobalAuditMapping item);
    bool Remove(string code);
    List<GlobalAuditMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalAuditMappingStore(Dictionary<string, GlobalAuditMapping> items, List<string> order) : IGlobalAuditMappingStore
{
    public Dictionary<string, GlobalAuditMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalAuditMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalAuditMapping item)
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

    public List<GlobalAuditMapping> List()
    {
        var outValue = new List<GlobalAuditMapping>();
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
public sealed class CachedGlobalAuditMappingStore(MemoryGlobalAuditMappingStore inner, GlobalAuditMappingCache cache, int hits, int misses) : IGlobalAuditMappingStore
{
    public MemoryGlobalAuditMappingStore Inner { get; set; } = inner;
    public GlobalAuditMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalAuditMapping? Find(string code)
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

    public void Save(GlobalAuditMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalAuditMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global audit mappings. Empty fields match all.</summary>
public sealed record GlobalAuditMappingQuery(string Department, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalAuditMapping item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<GlobalAuditMapping> Apply(List<GlobalAuditMapping> items)
    {
        var matched = Collections.Filter(items, (GlobalAuditMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global audit mapping store.</summary>
public static class GlobalAuditMappingStore
{
    public static IGlobalAuditMappingStore NewMemoryGlobalAuditMappingStore()
    {
        return new MemoryGlobalAuditMappingStore(new Dictionary<string, GlobalAuditMapping>(), new List<string>());
    }

    public static IGlobalAuditMappingStore NewCachedGlobalAuditMappingStore(MemoryGlobalAuditMappingStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalAuditMappingStore(inner, GlobalAuditMappingCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<GlobalAuditMapping> FindGlobalAuditMappings(IGlobalAuditMappingStore store, GlobalAuditMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
