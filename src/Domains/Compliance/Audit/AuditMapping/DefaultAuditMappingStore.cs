namespace Acme.Workforce.Compliance.Audit.AuditMapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default audit mappings by code.</summary>
public interface IDefaultAuditMappingStore
{
    DefaultAuditMapping? Find(string code);
    void Save(DefaultAuditMapping item);
    bool Remove(string code);
    List<DefaultAuditMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultAuditMappingStore(Dictionary<string, DefaultAuditMapping> items, List<string> order) : IDefaultAuditMappingStore
{
    public Dictionary<string, DefaultAuditMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultAuditMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultAuditMapping item)
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

    public List<DefaultAuditMapping> List()
    {
        var outValue = new List<DefaultAuditMapping>();
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
public sealed class CachedDefaultAuditMappingStore(MemoryDefaultAuditMappingStore inner, DefaultAuditMappingCache cache, int hits, int misses) : IDefaultAuditMappingStore
{
    public MemoryDefaultAuditMappingStore Inner { get; set; } = inner;
    public DefaultAuditMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultAuditMapping? Find(string code)
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

    public void Save(DefaultAuditMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultAuditMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default audit mappings. Empty fields match all.</summary>
public sealed record DefaultAuditMappingQuery(string Source, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultAuditMapping item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
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

    public List<DefaultAuditMapping> Apply(List<DefaultAuditMapping> items)
    {
        var matched = Collections.Filter(items, (DefaultAuditMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default audit mapping store.</summary>
public static class DefaultAuditMappingStore
{
    public static IDefaultAuditMappingStore NewMemoryDefaultAuditMappingStore()
    {
        return new MemoryDefaultAuditMappingStore(new Dictionary<string, DefaultAuditMapping>(), new List<string>());
    }

    public static IDefaultAuditMappingStore NewCachedDefaultAuditMappingStore(MemoryDefaultAuditMappingStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultAuditMappingStore(inner, DefaultAuditMappingCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<DefaultAuditMapping> FindDefaultAuditMappings(IDefaultAuditMappingStore store, DefaultAuditMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
