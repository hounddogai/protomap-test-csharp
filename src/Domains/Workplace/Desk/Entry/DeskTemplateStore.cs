namespace Acme.Workforce.Workplace.Desk.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps desk templates by code.</summary>
public interface IDeskTemplateStore
{
    DeskTemplate? Find(string code);
    void Save(DeskTemplate item);
    bool Remove(string code);
    List<DeskTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeskTemplateStore(Dictionary<string, DeskTemplate> items, List<string> order) : IDeskTemplateStore
{
    public Dictionary<string, DeskTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeskTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeskTemplate item)
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

    public List<DeskTemplate> List()
    {
        var outValue = new List<DeskTemplate>();
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
public sealed class CachedDeskTemplateStore(MemoryDeskTemplateStore inner, DeskTemplateCache cache, int hits, int misses) : IDeskTemplateStore
{
    public MemoryDeskTemplateStore Inner { get; set; } = inner;
    public DeskTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeskTemplate? Find(string code)
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

    public void Save(DeskTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeskTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects desk templates. Empty fields match all.</summary>
public sealed record DeskTemplateQuery(string Country, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(DeskTemplate item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DeskTemplate> Apply(List<DeskTemplate> items)
    {
        var matched = Collections.Filter(items, (DeskTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of desk template store.</summary>
public static class DeskTemplateStore
{
    public static IDeskTemplateStore NewMemoryDeskTemplateStore()
    {
        return new MemoryDeskTemplateStore(new Dictionary<string, DeskTemplate>(), new List<string>());
    }

    public static IDeskTemplateStore NewCachedDeskTemplateStore(MemoryDeskTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedDeskTemplateStore(inner, DeskTemplateCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<DeskTemplate> FindDeskTemplates(IDeskTemplateStore store, DeskTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
