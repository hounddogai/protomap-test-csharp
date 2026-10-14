namespace Acme.Workforce.Compliance.Certification.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps certification templates by code.</summary>
public interface ICertificationTemplateStore
{
    CertificationTemplate? Find(string code);
    void Save(CertificationTemplate item);
    bool Remove(string code);
    List<CertificationTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCertificationTemplateStore(Dictionary<string, CertificationTemplate> items, List<string> order) : ICertificationTemplateStore
{
    public Dictionary<string, CertificationTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CertificationTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CertificationTemplate item)
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

    public List<CertificationTemplate> List()
    {
        var outValue = new List<CertificationTemplate>();
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
public sealed class CachedCertificationTemplateStore(MemoryCertificationTemplateStore inner, CertificationTemplateCache cache, int hits, int misses) : ICertificationTemplateStore
{
    public MemoryCertificationTemplateStore Inner { get; set; } = inner;
    public CertificationTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CertificationTemplate? Find(string code)
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

    public void Save(CertificationTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CertificationTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects certification templates. Empty fields match all.</summary>
public sealed record CertificationTemplateQuery(string Label, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(CertificationTemplate item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<CertificationTemplate> Apply(List<CertificationTemplate> items)
    {
        var matched = Collections.Filter(items, (CertificationTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of certification template store.</summary>
public static class CertificationTemplateStore
{
    public static ICertificationTemplateStore NewMemoryCertificationTemplateStore()
    {
        return new MemoryCertificationTemplateStore(
            new Dictionary<string, CertificationTemplate>(),
            new List<string>());
    }

    public static ICertificationTemplateStore NewCachedCertificationTemplateStore(MemoryCertificationTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedCertificationTemplateStore(inner, CertificationTemplateCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<CertificationTemplate> FindCertificationTemplates(ICertificationTemplateStore store, CertificationTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
