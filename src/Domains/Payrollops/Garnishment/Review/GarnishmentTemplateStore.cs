namespace Acme.Workforce.Payrollops.Garnishment.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps garnishment templates by code.</summary>
public interface IGarnishmentTemplateStore
{
    GarnishmentTemplate? Find(string code);
    void Save(GarnishmentTemplate item);
    bool Remove(string code);
    List<GarnishmentTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGarnishmentTemplateStore(Dictionary<string, GarnishmentTemplate> items, List<string> order) : IGarnishmentTemplateStore
{
    public Dictionary<string, GarnishmentTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GarnishmentTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GarnishmentTemplate item)
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

    public List<GarnishmentTemplate> List()
    {
        var outValue = new List<GarnishmentTemplate>();
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
public sealed class CachedGarnishmentTemplateStore(MemoryGarnishmentTemplateStore inner, GarnishmentTemplateCache cache, int hits, int misses) : IGarnishmentTemplateStore
{
    public MemoryGarnishmentTemplateStore Inner { get; set; } = inner;
    public GarnishmentTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GarnishmentTemplate? Find(string code)
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

    public void Save(GarnishmentTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GarnishmentTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects garnishment templates. Empty fields match all.</summary>
public sealed record GarnishmentTemplateQuery(string Title, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(GarnishmentTemplate item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GarnishmentTemplate> Apply(List<GarnishmentTemplate> items)
    {
        var matched = Collections.Filter(items, (GarnishmentTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of garnishment template store.</summary>
public static class GarnishmentTemplateStore
{
    public static IGarnishmentTemplateStore NewMemoryGarnishmentTemplateStore()
    {
        return new MemoryGarnishmentTemplateStore(new Dictionary<string, GarnishmentTemplate>(), new List<string>());
    }

    public static IGarnishmentTemplateStore NewCachedGarnishmentTemplateStore(MemoryGarnishmentTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedGarnishmentTemplateStore(inner, GarnishmentTemplateCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<GarnishmentTemplate> FindGarnishmentTemplates(IGarnishmentTemplateStore store, GarnishmentTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
