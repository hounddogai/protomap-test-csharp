namespace Acme.Workforce.Compliance.Audit.AuditManifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary audit mappings by code.</summary>
public interface IPrimaryAuditMappingStore
{
    PrimaryAuditMapping? Find(string code);
    void Save(PrimaryAuditMapping item);
    bool Remove(string code);
    List<PrimaryAuditMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryAuditMappingStore(Dictionary<string, PrimaryAuditMapping> items, List<string> order) : IPrimaryAuditMappingStore
{
    public Dictionary<string, PrimaryAuditMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryAuditMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryAuditMapping item)
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

    public List<PrimaryAuditMapping> List()
    {
        var outValue = new List<PrimaryAuditMapping>();
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
public sealed class CachedPrimaryAuditMappingStore(MemoryPrimaryAuditMappingStore inner, PrimaryAuditMappingCache cache, int hits, int misses) : IPrimaryAuditMappingStore
{
    public MemoryPrimaryAuditMappingStore Inner { get; set; } = inner;
    public PrimaryAuditMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryAuditMapping? Find(string code)
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

    public void Save(PrimaryAuditMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryAuditMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary audit mappings. Empty fields match all.</summary>
public sealed record PrimaryAuditMappingQuery(string Locale, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryAuditMapping item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryAuditMapping> Apply(List<PrimaryAuditMapping> items)
    {
        var matched = Collections.Filter(items, (PrimaryAuditMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary audit mapping store.</summary>
public static class PrimaryAuditMappingStore
{
    public static IPrimaryAuditMappingStore NewMemoryPrimaryAuditMappingStore()
    {
        return new MemoryPrimaryAuditMappingStore(new Dictionary<string, PrimaryAuditMapping>(), new List<string>());
    }

    public static IPrimaryAuditMappingStore NewCachedPrimaryAuditMappingStore(MemoryPrimaryAuditMappingStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryAuditMappingStore(inner, PrimaryAuditMappingCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<PrimaryAuditMapping> FindPrimaryAuditMappings(IPrimaryAuditMappingStore store, PrimaryAuditMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
