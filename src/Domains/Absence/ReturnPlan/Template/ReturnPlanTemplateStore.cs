namespace Acme.Workforce.Absence.ReturnPlan.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps return plan templates by code.</summary>
public interface IReturnPlanTemplateStore
{
    ReturnPlanTemplate? Find(string code);
    void Save(ReturnPlanTemplate item);
    bool Remove(string code);
    List<ReturnPlanTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReturnPlanTemplateStore(Dictionary<string, ReturnPlanTemplate> items, List<string> order) : IReturnPlanTemplateStore
{
    public Dictionary<string, ReturnPlanTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReturnPlanTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReturnPlanTemplate item)
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

    public List<ReturnPlanTemplate> List()
    {
        var outValue = new List<ReturnPlanTemplate>();
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
public sealed class CachedReturnPlanTemplateStore(MemoryReturnPlanTemplateStore inner, ReturnPlanTemplateCache cache, int hits, int misses) : IReturnPlanTemplateStore
{
    public MemoryReturnPlanTemplateStore Inner { get; set; } = inner;
    public ReturnPlanTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReturnPlanTemplate? Find(string code)
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

    public void Save(ReturnPlanTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReturnPlanTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects return plan templates. Empty fields match all.</summary>
public sealed record ReturnPlanTemplateQuery(string Category, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(ReturnPlanTemplate item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<ReturnPlanTemplate> Apply(List<ReturnPlanTemplate> items)
    {
        var matched = Collections.Filter(items, (ReturnPlanTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of return plan template store.</summary>
public static class ReturnPlanTemplateStore
{
    public static IReturnPlanTemplateStore NewMemoryReturnPlanTemplateStore()
    {
        return new MemoryReturnPlanTemplateStore(new Dictionary<string, ReturnPlanTemplate>(), new List<string>());
    }

    public static IReturnPlanTemplateStore NewCachedReturnPlanTemplateStore(MemoryReturnPlanTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedReturnPlanTemplateStore(inner, ReturnPlanTemplateCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<ReturnPlanTemplate> FindReturnPlanTemplates(IReturnPlanTemplateStore store, ReturnPlanTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
