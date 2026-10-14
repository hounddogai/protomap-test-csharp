namespace Acme.Workforce.Workplace.Desk.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient desk templates by code.</summary>
public interface ILenientDeskTemplateStore
{
    LenientDeskTemplate? Find(string code);
    void Save(LenientDeskTemplate item);
    bool Remove(string code);
    List<LenientDeskTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientDeskTemplateStore(Dictionary<string, LenientDeskTemplate> items, List<string> order) : ILenientDeskTemplateStore
{
    public Dictionary<string, LenientDeskTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientDeskTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientDeskTemplate item)
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

    public List<LenientDeskTemplate> List()
    {
        var outValue = new List<LenientDeskTemplate>();
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
public sealed class CachedLenientDeskTemplateStore(MemoryLenientDeskTemplateStore inner, LenientDeskTemplateCache cache, int hits, int misses) : ILenientDeskTemplateStore
{
    public MemoryLenientDeskTemplateStore Inner { get; set; } = inner;
    public LenientDeskTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientDeskTemplate? Find(string code)
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

    public void Save(LenientDeskTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientDeskTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient desk templates. Empty fields match all.</summary>
public sealed record LenientDeskTemplateQuery(string Channel, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(LenientDeskTemplate item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientDeskTemplate> Apply(List<LenientDeskTemplate> items)
    {
        var matched = Collections.Filter(items, (LenientDeskTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient desk template store.</summary>
public static class LenientDeskTemplateStore
{
    public static ILenientDeskTemplateStore NewMemoryLenientDeskTemplateStore()
    {
        return new MemoryLenientDeskTemplateStore(new Dictionary<string, LenientDeskTemplate>(), new List<string>());
    }

    public static ILenientDeskTemplateStore NewCachedLenientDeskTemplateStore(MemoryLenientDeskTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedLenientDeskTemplateStore(inner, LenientDeskTemplateCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<LenientDeskTemplate> FindLenientDeskTemplates(ILenientDeskTemplateStore store, LenientDeskTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
