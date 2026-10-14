namespace Acme.Workforce.Workplace.Visitor.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps visitor templates by code.</summary>
public interface IVisitorTemplateStore
{
    VisitorTemplate? Find(string code);
    void Save(VisitorTemplate item);
    bool Remove(string code);
    List<VisitorTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVisitorTemplateStore(Dictionary<string, VisitorTemplate> items, List<string> order) : IVisitorTemplateStore
{
    public Dictionary<string, VisitorTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VisitorTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VisitorTemplate item)
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

    public List<VisitorTemplate> List()
    {
        var outValue = new List<VisitorTemplate>();
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
public sealed class CachedVisitorTemplateStore(MemoryVisitorTemplateStore inner, VisitorTemplateCache cache, int hits, int misses) : IVisitorTemplateStore
{
    public MemoryVisitorTemplateStore Inner { get; set; } = inner;
    public VisitorTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VisitorTemplate? Find(string code)
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

    public void Save(VisitorTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VisitorTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects visitor templates. Empty fields match all.</summary>
public sealed record VisitorTemplateQuery(string Department, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(VisitorTemplate item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VisitorTemplate> Apply(List<VisitorTemplate> items)
    {
        var matched = Collections.Filter(items, (VisitorTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of visitor template store.</summary>
public static class VisitorTemplateStore
{
    public static IVisitorTemplateStore NewMemoryVisitorTemplateStore()
    {
        return new MemoryVisitorTemplateStore(new Dictionary<string, VisitorTemplate>(), new List<string>());
    }

    public static IVisitorTemplateStore NewCachedVisitorTemplateStore(MemoryVisitorTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedVisitorTemplateStore(inner, VisitorTemplateCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<VisitorTemplate> FindVisitorTemplates(IVisitorTemplateStore store, VisitorTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
