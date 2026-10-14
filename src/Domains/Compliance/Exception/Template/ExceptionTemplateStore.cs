namespace Acme.Workforce.Compliance.Exception.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps exception templates by code.</summary>
public interface IExceptionTemplateStore
{
    ExceptionTemplate? Find(string code);
    void Save(ExceptionTemplate item);
    bool Remove(string code);
    List<ExceptionTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryExceptionTemplateStore(Dictionary<string, ExceptionTemplate> items, List<string> order) : IExceptionTemplateStore
{
    public Dictionary<string, ExceptionTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ExceptionTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ExceptionTemplate item)
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

    public List<ExceptionTemplate> List()
    {
        var outValue = new List<ExceptionTemplate>();
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
public sealed class CachedExceptionTemplateStore(MemoryExceptionTemplateStore inner, ExceptionTemplateCache cache, int hits, int misses) : IExceptionTemplateStore
{
    public MemoryExceptionTemplateStore Inner { get; set; } = inner;
    public ExceptionTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ExceptionTemplate? Find(string code)
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

    public void Save(ExceptionTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ExceptionTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects exception templates. Empty fields match all.</summary>
public sealed record ExceptionTemplateQuery(string Title, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(ExceptionTemplate item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ExceptionTemplate> Apply(List<ExceptionTemplate> items)
    {
        var matched = Collections.Filter(items, (ExceptionTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of exception template store.</summary>
public static class ExceptionTemplateStore
{
    public static IExceptionTemplateStore NewMemoryExceptionTemplateStore()
    {
        return new MemoryExceptionTemplateStore(new Dictionary<string, ExceptionTemplate>(), new List<string>());
    }

    public static IExceptionTemplateStore NewCachedExceptionTemplateStore(MemoryExceptionTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedExceptionTemplateStore(inner, ExceptionTemplateCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<ExceptionTemplate> FindExceptionTemplates(IExceptionTemplateStore store, ExceptionTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
