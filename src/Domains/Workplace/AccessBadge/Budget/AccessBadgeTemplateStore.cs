namespace Acme.Workforce.Workplace.AccessBadge.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps access badge templates by code.</summary>
public interface IAccessBadgeTemplateStore
{
    AccessBadgeTemplate? Find(string code);
    void Save(AccessBadgeTemplate item);
    bool Remove(string code);
    List<AccessBadgeTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccessBadgeTemplateStore(Dictionary<string, AccessBadgeTemplate> items, List<string> order) : IAccessBadgeTemplateStore
{
    public Dictionary<string, AccessBadgeTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccessBadgeTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccessBadgeTemplate item)
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

    public List<AccessBadgeTemplate> List()
    {
        var outValue = new List<AccessBadgeTemplate>();
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
public sealed class CachedAccessBadgeTemplateStore(MemoryAccessBadgeTemplateStore inner, AccessBadgeTemplateCache cache, int hits, int misses) : IAccessBadgeTemplateStore
{
    public MemoryAccessBadgeTemplateStore Inner { get; set; } = inner;
    public AccessBadgeTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccessBadgeTemplate? Find(string code)
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

    public void Save(AccessBadgeTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccessBadgeTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects access badge templates. Empty fields match all.</summary>
public sealed record AccessBadgeTemplateQuery(string Department, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(AccessBadgeTemplate item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AccessBadgeTemplate> Apply(List<AccessBadgeTemplate> items)
    {
        var matched = Collections.Filter(items, (AccessBadgeTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of access badge template store.</summary>
public static class AccessBadgeTemplateStore
{
    public static IAccessBadgeTemplateStore NewMemoryAccessBadgeTemplateStore()
    {
        return new MemoryAccessBadgeTemplateStore(new Dictionary<string, AccessBadgeTemplate>(), new List<string>());
    }

    public static IAccessBadgeTemplateStore NewCachedAccessBadgeTemplateStore(MemoryAccessBadgeTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedAccessBadgeTemplateStore(inner, AccessBadgeTemplateCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<AccessBadgeTemplate> FindAccessBadgeTemplates(IAccessBadgeTemplateStore store, AccessBadgeTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
