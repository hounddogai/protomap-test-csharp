namespace Acme.Workforce.Compliance.Audit.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps audit templates by code.</summary>
public interface IAuditTemplateStore
{
    AuditTemplate? Find(string code);
    void Save(AuditTemplate item);
    bool Remove(string code);
    List<AuditTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAuditTemplateStore(Dictionary<string, AuditTemplate> items, List<string> order) : IAuditTemplateStore
{
    public Dictionary<string, AuditTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AuditTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AuditTemplate item)
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

    public List<AuditTemplate> List()
    {
        var outValue = new List<AuditTemplate>();
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
public sealed class CachedAuditTemplateStore(MemoryAuditTemplateStore inner, AuditTemplateCache cache, int hits, int misses) : IAuditTemplateStore
{
    public MemoryAuditTemplateStore Inner { get; set; } = inner;
    public AuditTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AuditTemplate? Find(string code)
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

    public void Save(AuditTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AuditTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects audit templates. Empty fields match all.</summary>
public sealed record AuditTemplateQuery(string Unit, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(AuditTemplate item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AuditTemplate> Apply(List<AuditTemplate> items)
    {
        var matched = Collections.Filter(items, (AuditTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of audit template store.</summary>
public static class AuditTemplateStore
{
    public static IAuditTemplateStore NewMemoryAuditTemplateStore()
    {
        return new MemoryAuditTemplateStore(new Dictionary<string, AuditTemplate>(), new List<string>());
    }

    public static IAuditTemplateStore NewCachedAuditTemplateStore(MemoryAuditTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedAuditTemplateStore(inner, AuditTemplateCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<AuditTemplate> FindAuditTemplates(IAuditTemplateStore store, AuditTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
