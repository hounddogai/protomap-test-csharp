namespace Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped access badge templates by code.</summary>
public interface ICappedAccessBadgeTemplateStore
{
    CappedAccessBadgeTemplate? Find(string code);
    void Save(CappedAccessBadgeTemplate item);
    bool Remove(string code);
    List<CappedAccessBadgeTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedAccessBadgeTemplateStore(Dictionary<string, CappedAccessBadgeTemplate> items, List<string> order) : ICappedAccessBadgeTemplateStore
{
    public Dictionary<string, CappedAccessBadgeTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedAccessBadgeTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedAccessBadgeTemplate item)
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

    public List<CappedAccessBadgeTemplate> List()
    {
        var outValue = new List<CappedAccessBadgeTemplate>();
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
public sealed class CachedCappedAccessBadgeTemplateStore(MemoryCappedAccessBadgeTemplateStore inner, CappedAccessBadgeTemplateCache cache, int hits, int misses) : ICappedAccessBadgeTemplateStore
{
    public MemoryCappedAccessBadgeTemplateStore Inner { get; set; } = inner;
    public CappedAccessBadgeTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedAccessBadgeTemplate? Find(string code)
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

    public void Save(CappedAccessBadgeTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedAccessBadgeTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped access badge templates. Empty fields match all.</summary>
public sealed record CappedAccessBadgeTemplateQuery(string Source, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(CappedAccessBadgeTemplate item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedAccessBadgeTemplate> Apply(List<CappedAccessBadgeTemplate> items)
    {
        var matched = Collections.Filter(items, (CappedAccessBadgeTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped access badge template store.</summary>
public static class CappedAccessBadgeTemplateStore
{
    public static ICappedAccessBadgeTemplateStore NewMemoryCappedAccessBadgeTemplateStore()
    {
        return new MemoryCappedAccessBadgeTemplateStore(
            new Dictionary<string, CappedAccessBadgeTemplate>(),
            new List<string>());
    }

    public static ICappedAccessBadgeTemplateStore NewCachedCappedAccessBadgeTemplateStore(MemoryCappedAccessBadgeTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedCappedAccessBadgeTemplateStore(
            inner,
            CappedAccessBadgeTemplateCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<CappedAccessBadgeTemplate> FindCappedAccessBadgeTemplates(ICappedAccessBadgeTemplateStore store, CappedAccessBadgeTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
