namespace Acme.Workforce.Orgdesign.Position.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps position templates by code.</summary>
public interface IPositionTemplateStore
{
    PositionTemplate? Find(string code);
    void Save(PositionTemplate item);
    bool Remove(string code);
    List<PositionTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPositionTemplateStore(Dictionary<string, PositionTemplate> items, List<string> order) : IPositionTemplateStore
{
    public Dictionary<string, PositionTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PositionTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PositionTemplate item)
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

    public List<PositionTemplate> List()
    {
        var outValue = new List<PositionTemplate>();
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
public sealed class CachedPositionTemplateStore(MemoryPositionTemplateStore inner, PositionTemplateCache cache, int hits, int misses) : IPositionTemplateStore
{
    public MemoryPositionTemplateStore Inner { get; set; } = inner;
    public PositionTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PositionTemplate? Find(string code)
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

    public void Save(PositionTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PositionTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects position templates. Empty fields match all.</summary>
public sealed record PositionTemplateQuery(string Country, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(PositionTemplate item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PositionTemplate> Apply(List<PositionTemplate> items)
    {
        var matched = Collections.Filter(items, (PositionTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of position template store.</summary>
public static class PositionTemplateStore
{
    public static IPositionTemplateStore NewMemoryPositionTemplateStore()
    {
        return new MemoryPositionTemplateStore(new Dictionary<string, PositionTemplate>(), new List<string>());
    }

    public static IPositionTemplateStore NewCachedPositionTemplateStore(MemoryPositionTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedPositionTemplateStore(inner, PositionTemplateCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PositionTemplate> FindPositionTemplates(IPositionTemplateStore store, PositionTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
