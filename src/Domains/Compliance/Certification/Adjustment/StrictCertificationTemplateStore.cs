namespace Acme.Workforce.Compliance.Certification.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict certification templates by code.</summary>
public interface IStrictCertificationTemplateStore
{
    StrictCertificationTemplate? Find(string code);
    void Save(StrictCertificationTemplate item);
    bool Remove(string code);
    List<StrictCertificationTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictCertificationTemplateStore(Dictionary<string, StrictCertificationTemplate> items, List<string> order) : IStrictCertificationTemplateStore
{
    public Dictionary<string, StrictCertificationTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictCertificationTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictCertificationTemplate item)
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

    public List<StrictCertificationTemplate> List()
    {
        var outValue = new List<StrictCertificationTemplate>();
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
public sealed class CachedStrictCertificationTemplateStore(MemoryStrictCertificationTemplateStore inner, StrictCertificationTemplateCache cache, int hits, int misses) : IStrictCertificationTemplateStore
{
    public MemoryStrictCertificationTemplateStore Inner { get; set; } = inner;
    public StrictCertificationTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictCertificationTemplate? Find(string code)
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

    public void Save(StrictCertificationTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictCertificationTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict certification templates. Empty fields match all.</summary>
public sealed record StrictCertificationTemplateQuery(string Title, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(StrictCertificationTemplate item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictCertificationTemplate> Apply(List<StrictCertificationTemplate> items)
    {
        var matched = Collections.Filter(items, (StrictCertificationTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict certification template store.</summary>
public static class StrictCertificationTemplateStore
{
    public static IStrictCertificationTemplateStore NewMemoryStrictCertificationTemplateStore()
    {
        return new MemoryStrictCertificationTemplateStore(
            new Dictionary<string, StrictCertificationTemplate>(),
            new List<string>());
    }

    public static IStrictCertificationTemplateStore NewCachedStrictCertificationTemplateStore(MemoryStrictCertificationTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedStrictCertificationTemplateStore(
            inner,
            StrictCertificationTemplateCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<StrictCertificationTemplate> FindStrictCertificationTemplates(IStrictCertificationTemplateStore store, StrictCertificationTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
