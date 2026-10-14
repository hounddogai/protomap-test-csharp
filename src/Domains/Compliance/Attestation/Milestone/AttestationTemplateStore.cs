namespace Acme.Workforce.Compliance.Attestation.Milestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps attestation templates by code.</summary>
public interface IAttestationTemplateStore
{
    AttestationTemplate? Find(string code);
    void Save(AttestationTemplate item);
    bool Remove(string code);
    List<AttestationTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAttestationTemplateStore(Dictionary<string, AttestationTemplate> items, List<string> order) : IAttestationTemplateStore
{
    public Dictionary<string, AttestationTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AttestationTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AttestationTemplate item)
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

    public List<AttestationTemplate> List()
    {
        var outValue = new List<AttestationTemplate>();
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
public sealed class CachedAttestationTemplateStore(MemoryAttestationTemplateStore inner, AttestationTemplateCache cache, int hits, int misses) : IAttestationTemplateStore
{
    public MemoryAttestationTemplateStore Inner { get; set; } = inner;
    public AttestationTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AttestationTemplate? Find(string code)
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

    public void Save(AttestationTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AttestationTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects attestation templates. Empty fields match all.</summary>
public sealed record AttestationTemplateQuery(string Title, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(AttestationTemplate item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AttestationTemplate> Apply(List<AttestationTemplate> items)
    {
        var matched = Collections.Filter(items, (AttestationTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of attestation template store.</summary>
public static class AttestationTemplateStore
{
    public static IAttestationTemplateStore NewMemoryAttestationTemplateStore()
    {
        return new MemoryAttestationTemplateStore(new Dictionary<string, AttestationTemplate>(), new List<string>());
    }

    public static IAttestationTemplateStore NewCachedAttestationTemplateStore(MemoryAttestationTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedAttestationTemplateStore(inner, AttestationTemplateCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<AttestationTemplate> FindAttestationTemplates(IAttestationTemplateStore store, AttestationTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
