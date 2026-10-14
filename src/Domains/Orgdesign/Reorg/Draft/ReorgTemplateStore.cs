namespace Acme.Workforce.Orgdesign.Reorg.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps reorg templates by code.</summary>
public interface IReorgTemplateStore
{
    ReorgTemplate? Find(string code);
    void Save(ReorgTemplate item);
    bool Remove(string code);
    List<ReorgTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReorgTemplateStore(Dictionary<string, ReorgTemplate> items, List<string> order) : IReorgTemplateStore
{
    public Dictionary<string, ReorgTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReorgTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReorgTemplate item)
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

    public List<ReorgTemplate> List()
    {
        var outValue = new List<ReorgTemplate>();
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
public sealed class CachedReorgTemplateStore(MemoryReorgTemplateStore inner, ReorgTemplateCache cache, int hits, int misses) : IReorgTemplateStore
{
    public MemoryReorgTemplateStore Inner { get; set; } = inner;
    public ReorgTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReorgTemplate? Find(string code)
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

    public void Save(ReorgTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReorgTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects reorg templates. Empty fields match all.</summary>
public sealed record ReorgTemplateQuery(string Locale, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(ReorgTemplate item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
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

    public List<ReorgTemplate> Apply(List<ReorgTemplate> items)
    {
        var matched = Collections.Filter(items, (ReorgTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of reorg template store.</summary>
public static class ReorgTemplateStore
{
    public static IReorgTemplateStore NewMemoryReorgTemplateStore()
    {
        return new MemoryReorgTemplateStore(new Dictionary<string, ReorgTemplate>(), new List<string>());
    }

    public static IReorgTemplateStore NewCachedReorgTemplateStore(MemoryReorgTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedReorgTemplateStore(inner, ReorgTemplateCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<ReorgTemplate> FindReorgTemplates(IReorgTemplateStore store, ReorgTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
