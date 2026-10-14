namespace Acme.Workforce.Engagement.Recognition.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped recognition templates by code.</summary>
public interface ICappedRecognitionTemplateStore
{
    CappedRecognitionTemplate? Find(string code);
    void Save(CappedRecognitionTemplate item);
    bool Remove(string code);
    List<CappedRecognitionTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedRecognitionTemplateStore(Dictionary<string, CappedRecognitionTemplate> items, List<string> order) : ICappedRecognitionTemplateStore
{
    public Dictionary<string, CappedRecognitionTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedRecognitionTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedRecognitionTemplate item)
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

    public List<CappedRecognitionTemplate> List()
    {
        var outValue = new List<CappedRecognitionTemplate>();
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
public sealed class CachedCappedRecognitionTemplateStore(MemoryCappedRecognitionTemplateStore inner, CappedRecognitionTemplateCache cache, int hits, int misses) : ICappedRecognitionTemplateStore
{
    public MemoryCappedRecognitionTemplateStore Inner { get; set; } = inner;
    public CappedRecognitionTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedRecognitionTemplate? Find(string code)
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

    public void Save(CappedRecognitionTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedRecognitionTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped recognition templates. Empty fields match all.</summary>
public sealed record CappedRecognitionTemplateQuery(string Region, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(CappedRecognitionTemplate item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedRecognitionTemplate> Apply(List<CappedRecognitionTemplate> items)
    {
        var matched = Collections.Filter(items, (CappedRecognitionTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped recognition template store.</summary>
public static class CappedRecognitionTemplateStore
{
    public static ICappedRecognitionTemplateStore NewMemoryCappedRecognitionTemplateStore()
    {
        return new MemoryCappedRecognitionTemplateStore(
            new Dictionary<string, CappedRecognitionTemplate>(),
            new List<string>());
    }

    public static ICappedRecognitionTemplateStore NewCachedCappedRecognitionTemplateStore(MemoryCappedRecognitionTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedCappedRecognitionTemplateStore(
            inner,
            CappedRecognitionTemplateCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<CappedRecognitionTemplate> FindCappedRecognitionTemplates(ICappedRecognitionTemplateStore store, CappedRecognitionTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
